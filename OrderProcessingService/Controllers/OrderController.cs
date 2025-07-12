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

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, errorMessage, createdOrder) = await _orderService.CreateOrderAsync(order);
            if (!success)
                return BadRequest(new { error = errorMessage });

            return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder!.Id }, createdOrder);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new { error = "Order not found." });

            return Ok(order);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var orders = await _orderService.GetOrdersAsync(page, pageSize);
            return Ok(orders);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            {
                return BadRequest(new Dictionary<string, string>
        {
            { "error", "Invalid status value." }
        });
            }

            var success = await _orderService.UpdateOrderStatusAsync(id, newStatus);
            if (!success)
            {
                return NotFound(new Dictionary<string, string>
        {
            { "error", "Order not found." }
        });
            }

            return Ok(new Dictionary<string, string>
    {
        { "message", "Order status updated successfully." }
    });
        }
    }

    // DTO for updating order status
    //public class UpdateOrderStatusRequest
    //{
    //    public string Status { get; set; } = string.Empty;
    //}
}
