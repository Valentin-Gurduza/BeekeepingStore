using System;
using System.Web.Mvc;
using eUseControl.BeekeepingStore.BusinessLogic;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.Order;
using eUseControl.BeekeepingStore.Filters;
using eUseControl.BeekeepingStore.Domain.Enums;

namespace eUseControl.BeekeepingStore.Controllers
{
    [AdminMod]
    public class AdminOrderController : Controller
    {
        private readonly IOrder _orderBL;

        public AdminOrderController()
        {
            var bl = new BusinessLogic.BusinessLogic();
            _orderBL = bl.GetOrderBL;
        }

        // GET: AdminOrder
        public ActionResult Index(string status = null, int? userId = null, string sortBy = null, bool ascending = false)
        {
            // Get filtered orders
            var orders = _orderBL.GetFilteredOrders(status, userId, sortBy, ascending);

            // Setup view data for filtering
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentUserId = userId;
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortDirection = ascending;

            // Status list for filtering
            ViewBag.StatusList = new SelectList(new[]
            {
                new { Value = OrderStatus.Pending, Text = "Pending" },
                new { Value = OrderStatus.Processing, Text = "Processing" },
                new { Value = OrderStatus.Shipped, Text = "Shipped" },
                new { Value = OrderStatus.Delivered, Text = "Delivered" },
                new { Value = OrderStatus.Cancelled, Text = "Cancelled" },
                new { Value = OrderStatus.Returned, Text = "Returned" }
            }, "Value", "Text", status);

            return View(orders);
        }

        // GET: AdminOrder/Details/5
        public ActionResult Details(int id)
        {
            var order = _orderBL.GetOrderById(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: AdminOrder/UpdateStatus/5
        public ActionResult UpdateStatus(int id)
        {
            var order = _orderBL.GetOrderById(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            // Create view model for order status update
            var model = new UpdateOrderStatusRequest
            {
                OrderId = id,
                OrderStatus = order.OrderStatus
            };

            // Status list for dropdown
            ViewBag.StatusList = new SelectList(new[]
            {
                new { Value = OrderStatus.Pending, Text = "Pending" },
                new { Value = OrderStatus.Processing, Text = "Processing" },
                new { Value = OrderStatus.Shipped, Text = "Shipped" },
                new { Value = OrderStatus.Delivered, Text = "Delivered" },
                new { Value = OrderStatus.Cancelled, Text = "Cancelled" },
                new { Value = OrderStatus.Returned, Text = "Returned" }
            }, "Value", "Text", order.OrderStatus);

            return View(model);
        }

        // POST: AdminOrder/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(UpdateOrderStatusRequest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Update order status
                    bool success = _orderBL.UpdateOrderStatus(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Statusul comenzii a fost actualizat cu succes.";
                        return RedirectToAction("Details", new { id = model.OrderId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Nu s-a putut actualiza statusul comenzii.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "A apărut o eroare la actualizarea statusului: " + ex.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            var order = _orderBL.GetOrderById(model.OrderId);

            // Status list for dropdown
            ViewBag.StatusList = new SelectList(new[]
            {
                new { Value = OrderStatus.Pending, Text = "Pending" },
                new { Value = OrderStatus.Processing, Text = "Processing" },
                new { Value = OrderStatus.Shipped, Text = "Shipped" },
                new { Value = OrderStatus.Delivered, Text = "Delivered" },
                new { Value = OrderStatus.Cancelled, Text = "Cancelled" },
                new { Value = OrderStatus.Returned, Text = "Returned" }
            }, "Value", "Text", model.OrderStatus);

            return View(model);
        }

        // GET: AdminOrder/AddShipping/5
        public ActionResult AddShipping(int id)
        {
            var order = _orderBL.GetOrderById(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            // Create view model
            var model = new ShippingInfoViewModel
            {
                OrderId = id,
                TrackingNumber = order.TrackingNumber,
                ShippedDate = order.ShippedDate ?? DateTime.Now
            };

            return View(model);
        }

        // POST: AdminOrder/AddShipping
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddShipping(ShippingInfoViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Add shipping information
                    bool success = _orderBL.AddShippingInfo(model.OrderId, model.TrackingNumber, model.ShippedDate);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Informațiile de livrare au fost adăugate cu succes.";
                        return RedirectToAction("Details", new { id = model.OrderId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Nu s-au putut adăuga informațiile de livrare.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "A apărut o eroare la adăugarea informațiilor de livrare: " + ex.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // POST: AdminOrder/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            try
            {
                // Cancel order
                bool success = _orderBL.CancelOrder(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Comanda a fost anulată cu succes.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Nu s-a putut anula comanda. Verificați statusul comenzii.";
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "A apărut o eroare la anularea comenzii.";
                return RedirectToAction("Details", new { id });
            }
        }
    }

    // View model for shipping information
    public class ShippingInfoViewModel
    {
        public int OrderId { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Numărul de tracking este obligatoriu")]
        [System.ComponentModel.DataAnnotations.StringLength(100, ErrorMessage = "Numărul de tracking nu poate depăși 100 de caractere")]
        public string TrackingNumber { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Data expedierii este obligatorie")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Data expedierii")]
        public DateTime ShippedDate { get; set; }
    }
}