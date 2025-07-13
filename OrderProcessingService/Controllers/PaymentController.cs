using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderProcessingService.Models;
using OrderProcessingService.Repository;
using OrderProcessingService.Services;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
    {
        _logger.LogInformation("ProcessPayment called for OrderId: {OrderId} with Amount: {Amount} and PaymentMethod: {PaymentMethod}",
        request.OrderId, request.Amount, request.PaymentMethod);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for payment request. OrderId: {OrderId}, Amount: {Amount}, PaymentMethod: {PaymentMethod}",
            request.OrderId, request.Amount, request.PaymentMethod);
            return BadRequest(ModelState);
        }

        var (response, isSuccess) = await _paymentService.ProcessPaymentAsync(request);

        if (!isSuccess)
        {
            _logger.LogWarning("Payment failed for OrderId: {OrderId}. Message: {Message}", request.OrderId, response.Message);
            return BadRequest(response);
        }

        _logger.LogInformation("Payment processed successfully for OrderId: {OrderId}. TransactionId: {TransactionId}", request.OrderId, response.TransactionId);
        return Ok(response);
    }

    [HttpGet("{transactionId}")]
    public async Task<IActionResult> GetPaymentStatus(Guid transactionId)
    {
        _logger.LogInformation("GetPaymentStatus called for TransactionId: {TransactionId}", transactionId);

        var paymentTransaction = await _paymentService.GetPaymentStatusAsync(transactionId);
        if (paymentTransaction == null)
        {
            _logger.LogWarning("Payment transaction not found for TransactionId: {TransactionId}", transactionId);
            return NotFound(new { error = "Payment transaction not found." });
        }

        _logger.LogInformation("Payment status retrieved for TransactionId: {TransactionId}. Status: {Status}", transactionId, paymentTransaction.Status);
        return Ok(paymentTransaction);
    }
}