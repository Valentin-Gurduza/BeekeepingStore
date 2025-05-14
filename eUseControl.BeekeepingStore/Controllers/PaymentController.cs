using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using eUseControl.BeekeepingStore.BusinessLogic;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.Payment;
using eUseControl.BeekeepingStore.Filters;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;

namespace eUseControl.BeekeepingStore.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPayment _paymentBL;
        private readonly IOrder _orderBL;

        public PaymentController()
        {
            var bl = new BusinessLogic.BusinessLogic();
            _paymentBL = bl.GetPaymentBL;
            _orderBL = bl.GetOrderBL;
        }

        // GET: Payment/Process/{orderId}
        [UserMod]
        public ActionResult Process(int id)
        {
            try
            {
                // Get user ID from session
                int userId = Convert.ToInt32(Session["UserId"]);

                // Get order details
                var order = _orderBL.GetOrderById(id);
                if (order == null || order.UserId != userId)
                {
                    return HttpNotFound();
                }

                // Check if order is already paid
                if (order.PaymentStatus == Domain.Enums.PaymentStatus.Completed)
                {
                    TempData["SuccessMessage"] = "Această comandă a fost deja plătită.";
                    return RedirectToAction("Details", "Order", new { id = id });
                }

                // Create view model
                var model = new ProcessPaymentViewModel
                {
                    OrderId = order.OrderId,
                    Amount = order.TotalAmount,
                    Currency = "MDL",
                    CustomerName = order.CustomerName ?? "Client",
                    PaymentMethod = "card" // Default payment method
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Process: {ex}");
                TempData["ErrorMessage"] = "A apărut o eroare la încărcarea paginii de plată: " + ex.Message;
                return RedirectToAction("Details", "Order", new { id = id });
            }
        }

        // POST: Payment/ProcessCard
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserMod]
        public async Task<ActionResult> ProcessCard(CardPaymentViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var viewModel = new ProcessPaymentViewModel
                    {
                        OrderId = model.OrderId,
                        Amount = model.Amount,
                        Currency = model.Currency,
                        PaymentMethod = "card",
                        CustomerName = model.CustomerName
                    };
                    return View("Process", viewModel);
                }

                // Get user ID from session
                int userId = Convert.ToInt32(Session["UserId"]);

                // Get order details
                var order = _orderBL.GetOrderById(model.OrderId);
                if (order == null || order.UserId != userId)
                {
                    return HttpNotFound();
                }

                // Create payment request
                var request = new PaymentRequest
                {
                    OrderId = model.OrderId,
                    Amount = model.Amount,
                    Currency = model.Currency,
                    PaymentMethod = "card",
                    CardNumber = model.CardNumber,
                    CardHolderName = model.CardHolderName,
                    ExpiryMonth = model.ExpiryMonth,
                    ExpiryYear = model.ExpiryYear,
                    Cvv = model.Cvv,
                    CustomerEmail = model.CustomerEmail,
                    CustomerName = model.CustomerName,
                    Description = $"Plată comandă #{model.OrderId}"
                };

                // Process payment
                var response = await _paymentBL.ProcessPaymentAsync(request);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Plata a fost procesată cu succes!";
                    return RedirectToAction("Confirmation", "Order", new { id = model.OrderId });
                }
                else
                {
                    ModelState.AddModelError("", response.Message);
                    var viewModel = new ProcessPaymentViewModel
                    {
                        OrderId = model.OrderId,
                        Amount = model.Amount,
                        Currency = model.Currency,
                        PaymentMethod = "card",
                        CustomerName = model.CustomerName
                    };
                    return View("Process", viewModel);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ProcessCard: {ex}");
                ModelState.AddModelError("", "A apărut o eroare la procesarea plății: " + ex.Message);
                var viewModel = new ProcessPaymentViewModel
                {
                    OrderId = model.OrderId,
                    Amount = model.Amount,
                    Currency = model.Currency,
                    PaymentMethod = "card",
                    CustomerName = model.CustomerName
                };
                return View("Process", viewModel);
            }
        }

        // POST: Payment/ProcessTransfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserMod]
        public async Task<ActionResult> ProcessTransfer(TransferPaymentViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var viewModel = new ProcessPaymentViewModel
                    {
                        OrderId = model.OrderId,
                        Amount = model.Amount,
                        Currency = model.Currency,
                        PaymentMethod = "transfer",
                        CustomerName = model.CustomerName
                    };
                    return View("Process", viewModel);
                }

                // Get user ID from session
                int userId = Convert.ToInt32(Session["UserId"]);

                // Get order details
                var order = _orderBL.GetOrderById(model.OrderId);
                if (order == null || order.UserId != userId)
                {
                    return HttpNotFound();
                }

                // Create payment request
                var request = new PaymentRequest
                {
                    OrderId = model.OrderId,
                    Amount = model.Amount,
                    Currency = model.Currency,
                    PaymentMethod = "transfer",
                    CustomerEmail = model.CustomerEmail,
                    CustomerName = model.CustomerName,
                    Description = $"Plată comandă #{model.OrderId}"
                };

                // Process payment
                var response = await _paymentBL.ProcessPaymentAsync(request);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Instrucțiunile de plată prin transfer bancar au fost generate. Vă rugăm să efectuați transferul în cel mai scurt timp.";
                    return RedirectToAction("TransferInstructions", new { id = model.OrderId, paymentId = response.PaymentId });
                }
                else
                {
                    ModelState.AddModelError("", response.Message);
                    var viewModel = new ProcessPaymentViewModel
                    {
                        OrderId = model.OrderId,
                        Amount = model.Amount,
                        Currency = model.Currency,
                        PaymentMethod = "transfer",
                        CustomerName = model.CustomerName
                    };
                    return View("Process", viewModel);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ProcessTransfer: {ex}");
                ModelState.AddModelError("", "A apărut o eroare la procesarea plății: " + ex.Message);
                var viewModel = new ProcessPaymentViewModel
                {
                    OrderId = model.OrderId,
                    Amount = model.Amount,
                    Currency = model.Currency,
                    PaymentMethod = "transfer",
                    CustomerName = model.CustomerName
                };
                return View("Process", viewModel);
            }
        }

        // GET: Payment/TransferInstructions/{id}
        [UserMod]
        public ActionResult TransferInstructions(int id, string paymentId)
        {
            // Get user ID from session
            int userId = Convert.ToInt32(Session["UserId"]);

            // Get order details
            var order = _orderBL.GetOrderById(id);
            if (order == null || order.UserId != userId)
            {
                return HttpNotFound();
            }

            var model = new TransferInstructionsViewModel
            {
                OrderId = id,
                PaymentId = paymentId,
                Amount = order.TotalAmount,
                BankName = "Banca Comercială Română",
                AccountName = "BeekeepingStore SRL",
                AccountNumber = "RO49RNCB0082044172720001",
                Reference = $"Comandă #{id}"
            };

            return View(model);
        }

        // GET: Payment/VerifyStatus/{paymentId}
        [UserMod]
        public async Task<ActionResult> VerifyStatus(string paymentId)
        {
            try
            {
                var response = await _paymentBL.VerifyPaymentStatusAsync(paymentId);
                return Json(new { success = response.Success, status = response.Status, message = response.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in VerifyStatus: {ex}");
                return Json(new { success = false, message = "A apărut o eroare la verificarea statusului plății: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }

    #region View Models

    public class ProcessPaymentViewModel
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
    }

    public class CardPaymentViewModel
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        [Required(ErrorMessage = "Numărul cardului este obligatoriu")]
        [Display(Name = "Număr card")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Numele titularului este obligatoriu")]
        [Display(Name = "Titular card")]
        public string CardHolderName { get; set; }

        [Required(ErrorMessage = "Luna expirării este obligatorie")]
        [Display(Name = "Luna expirare")]
        public string ExpiryMonth { get; set; }

        [Required(ErrorMessage = "Anul expirării este obligatoriu")]
        [Display(Name = "An expirare")]
        public string ExpiryYear { get; set; }

        [Required(ErrorMessage = "Codul CVV este obligatoriu")]
        [Display(Name = "CVV")]
        public string Cvv { get; set; }

        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
    }

    public class TransferPaymentViewModel
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Adresa de email este obligatorie")]
        [EmailAddress(ErrorMessage = "Adresa de email nu este validă")]
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; }
    }

    public class TransferInstructionsViewModel
    {
        public int OrderId { get; set; }
        public string PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string BankName { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string Reference { get; set; }
    }

    #endregion
}