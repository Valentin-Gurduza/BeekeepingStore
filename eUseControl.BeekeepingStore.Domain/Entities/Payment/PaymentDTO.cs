using System;
using System.ComponentModel.DataAnnotations;

namespace eUseControl.BeekeepingStore.Domain.Entities.Payment
{
    public class PaymentRequest
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "MDL";

        [Required]
        public string PaymentMethod { get; set; }

        // Credit card details
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string Cvv { get; set; }

        // Bank transfer details
        public string BankAccount { get; set; }

        // Customer information
        public string CustomerEmail { get; set; }
        public string CustomerName { get; set; }

        // Additional information
        public string Description { get; set; }
    }

    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string PaymentId { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class PaymentStatusResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string PaymentId { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class RefundRequest
    {
        [Required]
        public string PaymentId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public string Reason { get; set; }
    }

    public class RefundResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string RefundId { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Amount { get; set; }
    }
}