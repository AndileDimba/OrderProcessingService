using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Data;
using OrderProcessingService.DTOs;
using OrderProcessingService.Models;
using OrderProcessingService.Services;
using Swashbuckle.AspNetCore.Filters;

namespace OrderProcessingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderProcessingContext _context;
        private readonly OrderService _orderService;

        public OrderController(OrderProcessingContext context, OrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        [HttpPost]
        [SwaggerRequestExample(typeof(Order), typeof(OrderExample))]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Custom validation
            if (order.Items == null || !order.Items.Any())
                return BadRequest("Order must contain at least one item.");

            if (order.Items.Any(i => i.Quantity <= 0 || i.UnitPrice <= 0))
                return BadRequest("Each item must have a positive quantity and unit price.");

            var calculatedTotal = order.Items.Sum(i => i.Quantity * i.UnitPrice);
            if (order.TotalAmount != calculatedTotal)
                return BadRequest($"TotalAmount ({order.TotalAmount}) does not match sum of items ({calculatedTotal}).");
            

            var (success, errorMessage) = await _orderService.ValidateAndReserveInventoryAsync(order);
            if (!success)
                return BadRequest(errorMessage);

            order.Id = Guid.NewGuid();
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            order.Status = OrderStatus.Pending;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        // GET /api/orders - List orders (with pagination)
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var orders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(orders);
        }

        // PUT /api/orders/{id}/status - Update order status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
                return BadRequest("Invalid status value.");

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(order);
        }
    }

    // DTO for updating order status
    //public class UpdateOrderStatusRequest
    //{
    //    public string Status { get; set; } = string.Empty;
    //}
}
