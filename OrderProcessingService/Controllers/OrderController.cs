using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Data;
using OrderProcessingService.DTOs;
using OrderProcessingService.Models;
using OrderProcessingService.Repository;
using OrderProcessingService.Services;
using Swashbuckle.AspNetCore.Filters;

namespace OrderProcessingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            _logger.LogInformation("CreateOrder called with TotalAmount: {TotalAmount} and ItemCount: {ItemCount}",
    order.TotalAmount, order.Items?.Count ?? 0);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for Order with TotalAmount: {TotalAmount} and ItemCount: {ItemCount}",
            order.TotalAmount, order.Items?.Count ?? 0);
                return BadRequest(ModelState);
            }

            var (success, errorMessage, createdOrder) = await _orderService.CreateOrderAsync(order);
            if (!success)
            {
                _logger.LogError("Failed to create order: {ErrorMessage}", errorMessage);
                return BadRequest(new { error = errorMessage });
            }

            _logger.LogInformation("Order created successfully with ID: {OrderId}", createdOrder!.Id);
            return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder!.Id }, createdOrder);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            _logger.LogInformation("GetOrderById called with ID: {OrderId}", id);

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Order not found with ID: {OrderId}", id);
                return NotFound(new { error = "Order not found." });
            }

            _logger.LogInformation("Order retrieved successfully with ID: {OrderId}", id);
            return Ok(order);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("GetOrders called to show {pageSize} different orders", pageSize);
            var orders = await _orderService.GetOrdersAsync(page, pageSize);
            return Ok(orders);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            _logger.LogInformation("UpdateOrderStatus called with ID: {OrderId} and Status: {Status}", id, request.Status);
            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            {
                _logger.LogWarning("Invalid status value: {Status}", request.Status);
                return BadRequest(new Dictionary<string, string>{{ "error", "Invalid status value." }});
            }

            var success = await _orderService.UpdateOrderStatusAsync(id, newStatus);
            if (!success)
            {
                _logger.LogWarning("Order not found with ID: {OrderId}", id);
                return NotFound(new Dictionary<string, string>{{ "error", "Order not found." }});
            }

            _logger.LogInformation("Order status updated successfully for ID: {OrderId} to Status: {Status}", id, newStatus);
            return Ok(new Dictionary<string, string>{{ "message", "Order status updated successfully." }});
        }
    }

    // DTO for updating order status
    //public class UpdateOrderStatusRequest
    //{
    //    public string Status { get; set; } = string.Empty;
    //}
}
