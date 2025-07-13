using OrderProcessingService.Data;
using OrderProcessingService.Models;
using OrderProcessingService.Repository;
using Microsoft.EntityFrameworkCore;
using OrderProcessingService.DTOs;
using Microsoft.Extensions.Options;

public class PaymentService : IPaymentService
{
    private readonly OrderProcessingContext _context;
    private readonly PaymentSettings _paymentSettings;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(OrderProcessingContext context, IOptions<PaymentSettings> paymentSettings, ILogger<PaymentService> logger)
    {
        _context = context;
        _paymentSettings = paymentSettings.Value;
        _logger = logger;
    }

    public async Task<(PaymentResponse Response, bool IsSuccess)> ProcessPaymentAsync(PaymentRequest request)
    {
        _logger.LogInformation("ProcessPaymentAsync called for OrderId: {OrderId} with Amount: {Amount} and PaymentMethod: {PaymentMethod}",
            request.OrderId, request.Amount, request.PaymentMethod);

        // Validate PaymentMethod
        if (!_paymentSettings.AllowedMethods.Contains(request.PaymentMethod))
        {
            _logger.LogWarning("Invalid payment method: {PaymentMethod} for OrderId: {OrderId}", request.PaymentMethod, request.OrderId);
            return (new PaymentResponse
            {
                Status = PaymentStatus.Failed.ToString(),
                Message = $"Invalid payment method: {request.PaymentMethod}. Allowed methods are: {string.Join(", ", _paymentSettings.AllowedMethods)}."
            }, false);
        }

        // Validate OrderId
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId);
        if (order == null)
        {
            _logger.LogWarning("Order not found for OrderId: {OrderId}", request.OrderId);
            return (new PaymentResponse
            {
                Status = PaymentStatus.Failed.ToString(),
                Message = $"Order with ID {request.OrderId} does not exist."
            }, false);
        }

        // Validate Amount
        if (order.TotalAmount != request.Amount)
        {
            _logger.LogWarning("Payment amount mismatch for OrderId: {OrderId}. Expected: {ExpectedAmount}, Actual: {ActualAmount}",
                request.OrderId, order.TotalAmount, request.Amount);
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

        if (isSuccess)
        {
            _logger.LogInformation("Payment processed successfully for OrderId: {OrderId}. TransactionId: {TransactionId}", request.OrderId, paymentTransaction.TransactionId);
        }
        else
        {
            _logger.LogWarning("Payment failed for OrderId: {OrderId}. TransactionId: {TransactionId}", request.OrderId, paymentTransaction.TransactionId);
        }

        return (new PaymentResponse
        {
            TransactionId = paymentTransaction.TransactionId,
            Status = paymentTransaction.Status.ToString(),
            Message = isSuccess ? "Payment processed successfully." : "Payment failed. Please try again."
        }, isSuccess);
    }

    public async Task<PaymentResponse?> GetPaymentStatusAsync(Guid transactionId)
    {
        _logger.LogInformation("GetPaymentStatusAsync called for TransactionId: {TransactionId}", transactionId);

        var paymentTransaction = await _context.PaymentTransactions.FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        if (paymentTransaction == null)
        {
            _logger.LogWarning("Payment transaction not found for TransactionId: {TransactionId}", transactionId);
            return null; // Return null if the transaction is not found
        }

        _logger.LogInformation("Payment status retrieved for TransactionId: {TransactionId}. Status: {Status}", transactionId, paymentTransaction.Status);
        return new PaymentResponse
        {
            TransactionId = paymentTransaction.TransactionId,
            Status = paymentTransaction.Status.ToString(),
            Message = "Payment status retrieved successfully."
        };
    }
}