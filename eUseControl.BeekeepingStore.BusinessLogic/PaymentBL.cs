using System;
using System.Diagnostics;
using System.Threading.Tasks;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.Order;
using eUseControl.BeekeepingStore.Domain.Entities.Payment;
using eUseControl.BeekeepingStore.Domain.Enums;
using System.Linq;
using System.Collections.Generic;

namespace eUseControl.BeekeepingStore.BusinessLogic
{
    public class PaymentBL : IPayment
    {
        /// <summary>
        /// Process a payment for an order
        /// </summary>
        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            Debug.WriteLine($"Processing payment for order {request.OrderId}, amount: {request.Amount} {request.Currency}");

            try
            {
                using (var db = new DataContext())
                {
                    // Check if order exists
                    var order = db.Orders.FirstOrDefault(o => o.OrderId == request.OrderId);
                    if (order == null)
                    {
                        Debug.WriteLine($"Order {request.OrderId} not found");
                        return new PaymentResponse
                        {
                            Success = false,
                            Message = $"Order {request.OrderId} not found",
                            Status = "failed",
                            Timestamp = DateTime.Now
                        };
                    }

                    // Check if payment already exists for this order
                    var existingPayment = db.Payments.FirstOrDefault(p => p.OrderId == request.OrderId);
                    if (existingPayment != null)
                    {
                        Debug.WriteLine($"Payment already exists for order {request.OrderId}");
                        return new PaymentResponse
                        {
                            Success = false,
                            Message = "Payment already processed for this order",
                            PaymentId = existingPayment.PaymentId.ToString(),
                            TransactionId = existingPayment.TransactionId,
                            Status = existingPayment.Status,
                            Timestamp = existingPayment.CreatedAt,
                            Amount = existingPayment.Amount,
                            Currency = existingPayment.Currency,
                            PaymentMethod = existingPayment.PaymentMethod
                        };
                    }

                    // Create payment record
                    var payment = new Payment
                    {
                        OrderId = request.OrderId,
                        Amount = request.Amount,
                        Currency = request.Currency,
                        PaymentMethod = request.PaymentMethod,
                        Status = "pending",
                        CreatedAt = DateTime.Now
                    };

                    // Process payment based on method
                    PaymentResponse response = null;

                    switch (request.PaymentMethod.ToLower())
                    {
                        case "card":
                            response = await ProcessCardPaymentAsync(request, payment);
                            break;
                        case "transfer":
                            response = await ProcessBankTransferAsync(request, payment);
                            break;
                        case "cash":
                            response = ProcessCashOnDelivery(request, payment);
                            break;
                        default:
                            response = new PaymentResponse
                            {
                                Success = false,
                                Message = $"Unsupported payment method: {request.PaymentMethod}",
                                Status = "failed",
                                Timestamp = DateTime.Now,
                                Amount = request.Amount,
                                Currency = request.Currency,
                                PaymentMethod = request.PaymentMethod
                            };
                            break;
                    }

                    // Update order payment status
                    if (response.Success)
                    {
                        order.PaymentStatus = response.Status == "completed" ?
                            PaymentStatus.Completed : PaymentStatus.Pending;
                    }
                    else
                    {
                        order.PaymentStatus = PaymentStatus.Failed;
                        payment.ErrorMessage = response.Message;
                    }

                    // Save payment to database
                    db.Payments.Add(payment);
                    db.SaveChanges();

                    // Update response with payment ID
                    response.PaymentId = payment.PaymentId.ToString();

                    return response;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ProcessPaymentAsync: {ex}");

                // Log error
                using (var db = new DataContext())
                {
                    db.ErrorLogs.Add(new ErrorLog
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        ErrorSource = ex.Source,
                        CreatedAt = DateTime.Now
                    });
                    db.SaveChanges();
                }

                return new PaymentResponse
                {
                    Success = false,
                    Message = $"Payment processing error: {ex.Message}",
                    Status = "error",
                    Timestamp = DateTime.Now,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    PaymentMethod = request.PaymentMethod
                };
            }
        }

        /// <summary>
        /// Process a credit card payment
        /// </summary>
        private async Task<PaymentResponse> ProcessCardPaymentAsync(PaymentRequest request, Payment payment)
        {
            Debug.WriteLine("Processing credit card payment");

            // Validate card details
            if (string.IsNullOrWhiteSpace(request.CardNumber) ||
                string.IsNullOrWhiteSpace(request.CardHolderName) ||
                string.IsNullOrWhiteSpace(request.ExpiryMonth) ||
                string.IsNullOrWhiteSpace(request.ExpiryYear) ||
                string.IsNullOrWhiteSpace(request.Cvv))
            {
                return new PaymentResponse
                {
                    Success = false,
                    Message = "Invalid card details",
                    Status = "failed",
                    Timestamp = DateTime.Now,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    PaymentMethod = request.PaymentMethod
                };
            }

            // Simulate payment gateway API call
            await Task.Delay(1500); // Simulate network delay

            // For testing purposes, we'll always approve payments
            bool isApproved = true;

            // Store masked card info
            payment.CardLast4 = request.CardNumber.Substring(request.CardNumber.Length - 4);
            payment.CardBrand = DetermineCardBrand(request.CardNumber);
            payment.TransactionId = $"CC-{Guid.NewGuid().ToString("N").Substring(0, 10)}";

            if (isApproved)
            {
                payment.Status = "completed";
                payment.TransactionDetails = $"Card payment approved. Auth code: {GenerateRandomAuthCode()}";

                return new PaymentResponse
                {
                    Success = true,
                    Message = "Payment approved",
                    Status = "completed",
                    TransactionId = payment.TransactionId,
                    Timestamp = DateTime.Now,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    PaymentMethod = request.PaymentMethod
                };
            }
            else
            {
                payment.Status = "failed";
                payment.TransactionDetails = "Card payment declined by issuer";
                payment.ErrorMessage = "Card declined";

                return new PaymentResponse
                {
                    Success = false,
                    Message = "Payment declined by card issuer",
                    Status = "failed",
                    TransactionId = payment.TransactionId,
                    Timestamp = DateTime.Now,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    PaymentMethod = request.PaymentMethod
                };
            }
        }

        /// <summary>
        /// Process a bank transfer payment
        /// </summary>
        private async Task<PaymentResponse> ProcessBankTransferAsync(PaymentRequest request, Payment payment)
        {
            Debug.WriteLine("Processing bank transfer payment");

            // In a real implementation, this would generate bank transfer instructions
            // For simulation, we'll just create a pending payment

            // Simulate processing delay
            await Task.Delay(1000);

            payment.Status = "pending";
            payment.TransactionId = $"BT-{Guid.NewGuid().ToString("N").Substring(0, 10)}";
            payment.TransactionDetails = "Bank transfer instructions sent to customer";

            return new PaymentResponse
            {
                Success = true,
                Message = "Bank transfer instructions generated. Payment pending.",
                Status = "pending",
                TransactionId = payment.TransactionId,
                Timestamp = DateTime.Now,
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentMethod = request.PaymentMethod
            };
        }

        /// <summary>
        /// Process a cash on delivery payment
        /// </summary>
        private PaymentResponse ProcessCashOnDelivery(PaymentRequest request, Payment payment)
        {
            Debug.WriteLine("Processing cash on delivery payment");

            // Cash on delivery is always pending until delivery
            payment.Status = "pending";
            payment.TransactionId = $"COD-{Guid.NewGuid().ToString("N").Substring(0, 10)}";
            payment.TransactionDetails = "Payment will be collected on delivery";

            return new PaymentResponse
            {
                Success = true,
                Message = "Order confirmed. Payment will be collected on delivery.",
                Status = "pending",
                TransactionId = payment.TransactionId,
                Timestamp = DateTime.Now,
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentMethod = request.PaymentMethod
            };
        }

        /// <summary>
        /// Verify the status of a payment
        /// </summary>
        public async Task<PaymentStatusResponse> VerifyPaymentStatusAsync(string paymentId)
        {
            Debug.WriteLine($"Verifying payment status for payment ID: {paymentId}");

            try
            {
                if (!int.TryParse(paymentId, out int paymentIdInt))
                {
                    return new PaymentStatusResponse
                    {
                        Success = false,
                        Message = "Invalid payment ID format",
                        Timestamp = DateTime.Now
                    };
                }

                using (var db = new DataContext())
                {
                    var payment = db.Payments.FirstOrDefault(p => p.PaymentId == paymentIdInt);
                    if (payment == null)
                    {
                        return new PaymentStatusResponse
                        {
                            Success = false,
                            Message = "Payment not found",
                            Timestamp = DateTime.Now
                        };
                    }

                    // For bank transfers, simulate checking for received payment
                    if (payment.PaymentMethod.ToLower() == "transfer" && payment.Status == "pending")
                    {
                        // Simulate checking with bank - 30% chance payment is received
                        await Task.Delay(1000); // Simulate network delay

                        if (new Random().Next(100) < 30)
                        {
                            // Payment received
                            payment.Status = "completed";
                            payment.UpdatedAt = DateTime.Now;
                            payment.TransactionDetails += "\nPayment received and confirmed.";

                            // Update order payment status
                            var order = db.Orders.FirstOrDefault(o => o.OrderId == payment.OrderId);
                            if (order != null)
                            {
                                order.PaymentStatus = PaymentStatus.Completed;
                            }

                            db.SaveChanges();
                        }
                    }

                    return new PaymentStatusResponse
                    {
                        Success = true,
                        PaymentId = payment.PaymentId.ToString(),
                        Status = payment.Status,
                        Message = "Payment status retrieved successfully",
                        Timestamp = payment.UpdatedAt ?? payment.CreatedAt
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in VerifyPaymentStatusAsync: {ex}");

                // Log error
                using (var db = new DataContext())
                {
                    db.ErrorLogs.Add(new ErrorLog
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        ErrorSource = ex.Source,
                        CreatedAt = DateTime.Now
                    });
                    db.SaveChanges();
                }

                return new PaymentStatusResponse
                {
                    Success = false,
                    Message = $"Error verifying payment status: {ex.Message}",
                    Timestamp = DateTime.Now
                };
            }
        }

        /// <summary>
        /// Refund a payment
        /// </summary>
        public async Task<RefundResponse> RefundPaymentAsync(RefundRequest request)
        {
            Debug.WriteLine($"Processing refund for payment ID: {request.PaymentId}");

            try
            {
                if (!int.TryParse(request.PaymentId, out int paymentIdInt))
                {
                    return new RefundResponse
                    {
                        Success = false,
                        Message = "Invalid payment ID format",
                        Timestamp = DateTime.Now
                    };
                }

                using (var db = new DataContext())
                {
                    var payment = db.Payments.FirstOrDefault(p => p.PaymentId == paymentIdInt);
                    if (payment == null)
                    {
                        return new RefundResponse
                        {
                            Success = false,
                            Message = "Payment not found",
                            Timestamp = DateTime.Now
                        };
                    }

                    // Check if payment is completed
                    if (payment.Status != "completed")
                    {
                        return new RefundResponse
                        {
                            Success = false,
                            Message = $"Cannot refund payment in status: {payment.Status}",
                            Timestamp = DateTime.Now
                        };
                    }

                    // Check if refund amount is valid
                    if (request.Amount <= 0 || request.Amount > payment.Amount)
                    {
                        return new RefundResponse
                        {
                            Success = false,
                            Message = $"Invalid refund amount. Must be between 0 and {payment.Amount}",
                            Timestamp = DateTime.Now
                        };
                    }

                    // Simulate refund processing
                    await Task.Delay(1500);

                    // Update payment record
                    payment.Status = "refunded";
                    payment.UpdatedAt = DateTime.Now;
                    payment.TransactionDetails += $"\nRefunded {request.Amount} {payment.Currency}. Reason: {request.Reason}";

                    // Update order payment status
                    var order = db.Orders.FirstOrDefault(o => o.OrderId == payment.OrderId);
                    if (order != null)
                    {
                        order.PaymentStatus = PaymentStatus.Refunded;
                    }

                    db.SaveChanges();

                    return new RefundResponse
                    {
                        Success = true,
                        Message = "Refund processed successfully",
                        RefundId = $"RF-{Guid.NewGuid().ToString("N").Substring(0, 10)}",
                        Status = "completed",
                        Timestamp = DateTime.Now,
                        Amount = request.Amount
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in RefundPaymentAsync: {ex}");

                // Log error
                using (var db = new DataContext())
                {
                    db.ErrorLogs.Add(new ErrorLog
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        ErrorSource = ex.Source,
                        CreatedAt = DateTime.Now
                    });
                    db.SaveChanges();
                }

                return new RefundResponse
                {
                    Success = false,
                    Message = $"Error processing refund: {ex.Message}",
                    Timestamp = DateTime.Now
                };
            }
        }

        #region Helper Methods

        /// <summary>
        /// Determine the card brand based on the card number
        /// </summary>
        private string DetermineCardBrand(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return "Unknown";

            // Remove spaces and dashes
            cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

            // Check card type based on prefix
            if (cardNumber.StartsWith("4"))
                return "Visa";
            else if (cardNumber.StartsWith("5"))
                return "MasterCard";
            else if (cardNumber.StartsWith("34") || cardNumber.StartsWith("37"))
                return "American Express";
            else if (cardNumber.StartsWith("6"))
                return "Discover";
            else
                return "Unknown";
        }

        /// <summary>
        /// Generate a random authorization code for card payments
        /// </summary>
        private string GenerateRandomAuthCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #endregion

        // Add the implementations for dashboard functionality at the end of the class

        public List<Payment> GetFilteredPayments(string searchTerm, string status, int page, int pageSize, out int totalCount)
        {
            using (var context = new DataContext())
            {
                var query = context.Payments.AsQueryable();

                // Căutare
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p =>
                        p.TransactionId.Contains(searchTerm) ||
                        p.PaymentMethod.Contains(searchTerm));
                }

                // Filtrare după status
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(p => p.Status == status);
                }

                // Get total count
                totalCount = query.Count();

                // Apply paging
                return query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
        }

        public Payment GetPaymentById(int paymentId)
        {
            using (var context = new DataContext())
            {
                return context.Payments.Find(paymentId);
            }
        }

        public bool UpdatePaymentStatus(int paymentId, string status)
        {
            using (var context = new DataContext())
            {
                var payment = context.Payments.Find(paymentId);

                if (payment == null)
                {
                    return false;
                }

                payment.Status = status;
                payment.UpdatedAt = DateTime.Now;
                context.SaveChanges();

                return true;
            }
        }

        public List<PaymentMethodStat> GetPaymentMethodStats()
        {
            using (var context = new DataContext())
            {
                return context.Payments
                    .GroupBy(p => p.PaymentMethod)
                    .Select(g => new PaymentMethodStat
                    {
                        Method = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(p => p.Amount)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();
            }
        }
    }
}