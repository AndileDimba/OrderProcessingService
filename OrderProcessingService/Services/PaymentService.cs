using OrderProcessingService.Data;
using OrderProcessingService.Models;
using OrderProcessingService.Repository;
using Microsoft.EntityFrameworkCore;

public class PaymentService : IPaymentService
{
    private readonly OrderProcessingContext _context;

    public PaymentService(OrderProcessingContext context)
    {
        _context = context;
    }

    public async Task<(PaymentResponse Response, bool IsSuccess)> ProcessPaymentAsync(PaymentRequest request)
    {
        // Validate OrderId
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId);
        if (order == null)
        {
            return (new PaymentResponse
            {
                Status = PaymentStatus.Failed.ToString(),
                Message = $"Order with ID {request.OrderId} does not exist."
            }, false);
        }

        // Validate Amount
        if (order.TotalAmount != request.Amount)
        {
            return (new PaymentResponse
            {
                Status = PaymentStatus.Failed.ToString(),
                Message = $"Payment amount ({request.Amount}) does not match the order total ({order.TotalAmount})."
            }, false);
        }

        // Simulate payment processing
        var random = new Random();
        var isSuccess = random.Next(0, 2) == 1; // 50% chance of success
        var status = isSuccess ? PaymentStatus.Completed : PaymentStatus.Failed;

        // Save payment transaction
        var paymentTransaction = new PaymentTransaction
        {
            TransactionId = Guid.NewGuid(),
            OrderId = request.OrderId.ToString(),
            Amount = request.Amount,
            Status = status,
            ProcessedAt = DateTime.UtcNow
        };

        _context.PaymentTransactions.Add(paymentTransaction);
        await _context.SaveChangesAsync();

        return (new PaymentResponse
        {
            TransactionId = paymentTransaction.TransactionId,
            Status = paymentTransaction.Status.ToString(),
            Message = isSuccess ? "Payment processed successfully." : "Payment failed. Please try again."
        }, isSuccess);
    }

    public async Task<PaymentResponse?> GetPaymentStatusAsync(Guid transactionId)
    {
        var paymentTransaction = await _context.PaymentTransactions.FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        if (paymentTransaction == null)
        {
            return null; // Return null if the transaction is not found
        }

        return new PaymentResponse
        {
            TransactionId = paymentTransaction.TransactionId,
            Status = paymentTransaction.Status.ToString(),
            Message = "Payment status retrieved successfully."
        };
    }
}