using eUseControl.BeekeepingStore.Domain.Entities.Payment;
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
    }
}