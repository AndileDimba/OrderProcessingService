using Microsoft.AspNetCore.Mvc;
using OrderProcessingService.Models;
using OrderProcessingService.Repository;
using OrderProcessingService.Services;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (response, isSuccess) = await _paymentService.ProcessPaymentAsync(request);
        return isSuccess ? Ok(response) : BadRequest(response);
    }

    [HttpGet("{transactionId}")]
    public async Task<IActionResult> GetPaymentStatus(Guid transactionId)
    {
        var paymentTransaction = await _paymentService.GetPaymentStatusAsync(transactionId);
        if (paymentTransaction == null)
            return NotFound(new { error = "Payment transaction not found." });

        return Ok(paymentTransaction);
    }
}