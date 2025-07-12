using OrderProcessingService.Models;

namespace OrderProcessingService.Repository
{
    public interface IPaymentService
    {
        Task<(PaymentResponse Response, bool IsSuccess)> ProcessPaymentAsync(PaymentRequest request);
        Task<PaymentResponse?> GetPaymentStatusAsync(Guid transactionId);
    }
}
