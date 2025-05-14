using System;
using System.Web.Mvc;
using eUseControl.BeekeepingStore.BusinessLogic;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.Order;
using eUseControl.BeekeepingStore.Filters;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Web;

namespace eUseControl.BeekeepingStore.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrder _orderBL;
        private readonly IProduct _productBL;

        public OrderController()
        {
            var bl = new BusinessLogic.BusinessLogic();
            _orderBL = bl.GetOrderBL;
            _productBL = bl.GetProductBL;
        }

        // GET: Order/Checkout
        [UserMod]
        public ActionResult Checkout()
        {
            // User ID from session
            int userId = Convert.ToInt32(Session["UserId"]);

            return View();
        }

        // POST: Order/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserMod]
        public ActionResult PlaceOrder(string cartItems, string shippingAddress, string billingAddress, string paymentMethod, string notes)
        {
            try
            {
                Debug.WriteLine("===== PlaceOrder: START =====");
                Debug.WriteLine($"PlaceOrder: cartItems={cartItems?.Length ?? 0} chars");
                Debug.WriteLine($"PlaceOrder: shippingAddress={shippingAddress}");
                Debug.WriteLine($"PlaceOrder: billingAddress={billingAddress}");
                Debug.WriteLine($"PlaceOrder: paymentMethod={paymentMethod}");
                Debug.WriteLine($"PlaceOrder: notes={notes}");

                // Verifică parametrii de intrare
                if (string.IsNullOrEmpty(cartItems))
                {
                    Debug.WriteLine("PlaceOrder: cartItems este null sau gol!");
                    return Json(new { success = false, message = "Coșul de cumpărături nu conține produse." });
                }

                // Get user ID from session
                Debug.WriteLine("PlaceOrder: Obținere UserId din sesiune");
                int userId;
                try
                {
                    userId = Convert.ToInt32(Session["UserId"]);
                    Debug.WriteLine($"PlaceOrder: UserId={userId}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PlaceOrder: Eroare la obținerea UserId: {ex.Message}");
                    return Json(new { success = false, message = "Eroare la identificarea utilizatorului. Te rugăm să te autentifici din nou." });
                }

                // Deserialize cart items
                Debug.WriteLine("PlaceOrder: Deserializare coș de cumpărături");
                List<CartItem> items;
                try
                {
                    items = JsonConvert.DeserializeObject<List<CartItem>>(cartItems);
                    if (items == null || items.Count == 0)
                    {
                        Debug.WriteLine("PlaceOrder: Coșul de cumpărături este gol");
                        return Json(new { success = false, message = "Coșul de cumpărături este gol" });
                    }
                    Debug.WriteLine($"PlaceOrder: {items.Count} produse în coș");
                    foreach (var item in items)
                    {
                        Debug.WriteLine($"  - Produs: ID={item.Id}, Name={item.Name}, Quantity={item.Quantity}, Price={item.Price}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PlaceOrder: Eroare la deserializarea coșului: {ex.Message}");
                    Debug.WriteLine($"PlaceOrder: Conținut coș: {cartItems}");
                    return Json(new { success = false, message = "Eroare la procesarea coșului de cumpărături: " + ex.Message });
                }

                // Create order request
                Debug.WriteLine("PlaceOrder: Creare cerere de comandă");
                var orderRequest = new CreateOrderRequest
                {
                    UserId = userId,
                    ShippingAddress = shippingAddress,
                    BillingAddress = billingAddress,
                    PaymentMethod = paymentMethod,
                    Notes = notes,
                    Items = new List<OrderItemRequest>()
                };

                // Add items to order
                Debug.WriteLine("PlaceOrder: Adăugare produse la comandă");
                foreach (var item in items)
                {
                    Debug.WriteLine($"PlaceOrder: Adaugă produs ID={item.Id}, Cantitate={item.Quantity}");
                    orderRequest.Items.Add(new OrderItemRequest
                    {
                        ProductId = item.Id,
                        Quantity = item.Quantity
                    });
                }

                // Create order
                Debug.WriteLine("PlaceOrder: Apel _orderBL.CreateOrder");
                int orderId;
                try
                {
                    orderId = _orderBL.CreateOrder(orderRequest);
                    Debug.WriteLine($"PlaceOrder: Comandă creată cu ID={orderId}");

                    // If payment method is cash on delivery, we're done
                    if (paymentMethod.ToLower() == "cash")
                    {
                        Debug.WriteLine("PlaceOrder: Metoda de plată este plata la livrare, nu este necesară procesarea plății");
                    }
                }
                catch (ArgumentException ex)
                {
                    Debug.WriteLine($"PlaceOrder: ArgumentException: {ex.Message}");
                    return Json(new { success = false, message = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    Debug.WriteLine($"PlaceOrder: InvalidOperationException: {ex.Message}");
                    return Json(new { success = false, message = ex.Message });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PlaceOrder: Exception la CreateOrder: {ex.GetType().Name}: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Debug.WriteLine($"PlaceOrder: InnerException: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                    }
                    return Json(new { success = false, message = "Eroare la crearea comenzii: " + ex.Message });
                }

                // Return success response
                Debug.WriteLine("PlaceOrder: Returnare răspuns de succes");
                return Json(new { success = true, orderId = orderId, message = "Comanda a fost plasată cu succes!" });
            }
            catch (Exception ex)
            {
                // General error
                Debug.WriteLine($"PlaceOrder: Exception generală: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"PlaceOrder: StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"PlaceOrder: InnerException: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                    Debug.WriteLine($"PlaceOrder: InnerException StackTrace: {ex.InnerException.StackTrace}");
                }

                // Log to file for debugging
                try
                {
                    string logPath = HttpContext.Server.MapPath("~/App_Data/order_errors.log");
                    System.IO.File.AppendAllText(logPath,
                        $"[{DateTime.Now}] ERROR: {ex.GetType().Name}: {ex.Message}\r\n{ex.StackTrace}\r\n\r\n");
                }
                catch (Exception logEx)
                {
                    Debug.WriteLine($"PlaceOrder: Nu s-a putut scrie în fișierul de log: {logEx.Message}");
                }

                return Json(new { success = false, message = "A apărut o eroare la plasarea comenzii: " + ex.Message });
            }
            finally
            {
                Debug.WriteLine("===== PlaceOrder: END =====");
            }
        }

        // GET: Order/Confirmation/{id}
        [UserMod]
        public ActionResult Confirmation(int id)
        {
            // Get user ID from session
            int userId = Convert.ToInt32(Session["UserId"]);

            // Get order details
            var order = _orderBL.GetOrderById(id);
            if (order == null || order.UserId != userId)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // GET: Order/Details/{id}
        [UserMod]
        public ActionResult Details(int id)
        {
            // Get user ID from session
            int userId = Convert.ToInt32(Session["UserId"]);

            // Get order details
            var order = _orderBL.GetOrderById(id);
            if (order == null || order.UserId != userId)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // GET: Order/History
        [UserMod]
        public ActionResult History()
        {
            // Get user ID from session
            int userId = Convert.ToInt32(Session["UserId"]);

            // Get user orders
            var orders = _orderBL.GetOrdersByUserId(userId);

            return View(orders);
        }

        // POST: Order/Cancel/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserMod]
        public ActionResult Cancel(int id)
        {
            try
            {
                // Get user ID from session
                int userId = Convert.ToInt32(Session["UserId"]);

                // Ensure order belongs to user
                var order = _orderBL.GetOrderById(id);
                if (order == null || order.UserId != userId)
                {
                    return HttpNotFound();
                }

                // Cancel order
                bool success = _orderBL.CancelOrder(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Comanda a fost anulată cu succes.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Nu s-a putut anula comanda.";
                }

                return RedirectToAction("History");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "A apărut o eroare la anularea comenzii.";
                return RedirectToAction("History");
            }
        }

        // Metodă simplă de test pentru a verifica dacă OrderController funcționează
        [HttpGet]
        public ActionResult TestConnection()
        {
            return Content("OrderController funcționează corect!");
        }

        // Metodă de test pentru a verifica dacă cererea POST pentru PlaceOrder funcționează
        [HttpGet]
        public ActionResult TestPlaceOrder()
        {
            try
            {
                Debug.WriteLine("===== TestPlaceOrder: START =====");
                return Content("Testarea metodei PlaceOrder funcționează! Folosește acest endpoint doar pentru debugging.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TestPlaceOrder: Eroare: {ex.Message}");
                return Content("Eroare: " + ex.Message);
            }
            finally
            {
                Debug.WriteLine("===== TestPlaceOrder: END =====");
            }
        }
    }

    // Helper class for cart items deserialization
    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; }

        public override string ToString()
        {
            return $"CartItem {{ Id={Id}, Name='{Name}', Price={Price}, Quantity={Quantity} }}";
        }
    }
}