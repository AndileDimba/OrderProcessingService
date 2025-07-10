using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Data;
using OrderProcessingService.Models;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly OrderProcessingContext _context;

    public PaymentController(OrderProcessingContext context)
    {
        _context = context;
    }

    // POST /api/payments/process
    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Validate OrderId
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId);
        if (order == null)
            return BadRequest($"Order with ID {request.OrderId} does not exist.");

        // Validate Amount
        if (order.TotalAmount != request.Amount)
            return BadRequest($"Payment amount ({request.Amount}) does not match the order total ({order.TotalAmount}).");

        // Simulate payment processing
        var random = new Random();
        var isSuccess = random.Next(0, 2) == 1; // 50% chance of success
        var status = isSuccess ? PaymentStatus.Completed : PaymentStatus.Failed;

        // Save payment transaction to the database
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

        // Return response
        var response = new PaymentResponse
        {
            TransactionId = paymentTransaction.TransactionId,
            Status = paymentTransaction.Status.ToString(),
            Message = isSuccess ? "Payment processed successfully." : "Payment failed. Please try again."
        };

        return isSuccess ? Ok(response) : BadRequest(response);
    }

    // GET /api/payments/{transactionId}
    [HttpGet("{transactionId}")]
    public async Task<IActionResult> GetPaymentStatus(Guid transactionId)
    {
        var paymentTransaction = await _context.PaymentTransactions.FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        if (paymentTransaction == null)
            return NotFound(new { error = "Payment transaction not found." });

        return Ok(new PaymentResponse
        {
            TransactionId = paymentTransaction.TransactionId,
            Status = paymentTransaction.Status.ToString(),
            Message = "Payment status retrieved successfully."
        });
    }
}