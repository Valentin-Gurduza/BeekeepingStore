using eUseControl.BeekeepingStore.Domain.Entities.Payment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eUseControl.BeekeepingStore.BusinessLogic.Interfaces
{
    public interface IPayment
    {
        /// <summary>
        /// Process a payment for an order
        /// </summary>
        /// <param name="request">Payment request details</param>
        /// <returns>Payment response with result</returns>
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);

        /// <summary>
        /// Verify the status of a payment
        /// </summary>
        /// <param name="paymentId">Payment ID to verify</param>
        /// <returns>Payment status response</returns>
        Task<PaymentStatusResponse> VerifyPaymentStatusAsync(string paymentId);

        /// <summary>
        /// Refund a payment
        /// </summary>
        /// <param name="request">Refund request details</param>
        /// <returns>Refund response with result</returns>
        Task<RefundResponse> RefundPaymentAsync(RefundRequest request);

        /// <summary>
        /// Get filtered payments with pagination
        /// </summary>
        /// <param name="searchTerm">Optional search term</param>
        /// <param name="status">Optional status filter</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalCount">Output parameter for total count</param>
        /// <returns>List of payments matching criteria</returns>
        List<Payment> GetFilteredPayments(string searchTerm, string status, int page, int pageSize, out int totalCount);

        /// <summary>
        /// Get payment by ID
        /// </summary>
        /// <param name="paymentId">Payment ID</param>
        /// <returns>Payment or null if not found</returns>
        Payment GetPaymentById(int paymentId);

        /// <summary>
        /// Update payment status
        /// </summary>
        /// <param name="paymentId">Payment ID</param>
        /// <param name="status">New status</param>
        /// <returns>true if successful, false if payment not found</returns>
        bool UpdatePaymentStatus(int paymentId, string status);

        /// <summary>
        /// Get payment method statistics
        /// </summary>
        /// <returns>List of payment method stats</returns>
        List<PaymentMethodStat> GetPaymentMethodStats();
    }

    public class PaymentMethodStat
    {
        public string Method { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }
}