using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Data;
using OrderProcessingService.Models;
using OrderProcessingService.DTOs;

namespace OrderProcessingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly OrderProcessingContext _context;

        public InventoryController(OrderProcessingContext context)
        {
            _context = context;
        }

        // GET /api/inventory/{productId} - Check product availability
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductAvailability(string productId)
        {
            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (item == null)
                return NotFound(new { error = $"Product {productId} not found in inventory." });

            return Ok(new
            {
                productId = item.ProductId,
                availableQuantity = item.AvailableQuantity,
                reservedQuantity = item.ReservedQuantity
            });
        }

        // POST /api/inventory/{productId}/reserve - Reserve items
        [HttpPost("{productId}/reserve")]
        public async Task<IActionResult> ReserveItems(string productId, [FromBody] ReserveInventoryRequest request)
        {
            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (item == null)
                return NotFound(new { error = $"Product {productId} not found in inventory." });

            if (request.Quantity <= 0)
                return BadRequest(new { error = "Quantity must be greater than zero." });

            if (item.AvailableQuantity < request.Quantity)
                return BadRequest(new { error = "Insufficient available quantity." });

            item.AvailableQuantity -= request.Quantity;
            item.ReservedQuantity += request.Quantity;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                productId = item.ProductId,
                availableQuantity = item.AvailableQuantity,
                reservedQuantity = item.ReservedQuantity
            });
        }

        // POST /api/inventory/{productId}/release - Release reserved items
        [HttpPost("{productId}/release")]
        public async Task<IActionResult> ReleaseItems(string productId, [FromBody] ReleaseInventoryRequest request)
        {
            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (item == null)
                return NotFound(new { error = $"Product {productId} not found in inventory." });

            if (request.Quantity <= 0)
                return BadRequest(new { error = "Quantity must be greater than zero." });

            if (item.ReservedQuantity < request.Quantity)
                return BadRequest(new { error = "Insufficient reserved quantity to release." });

            item.ReservedQuantity -= request.Quantity;
            item.AvailableQuantity += request.Quantity;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                productId = item.ProductId,
                availableQuantity = item.AvailableQuantity,
                reservedQuantity = item.ReservedQuantity
            });
        }
    }

    // Request DTOs
    //public class ReserveInventoryRequest
    //{
    //    public int Quantity { get; set; }
    //}

    //public class ReleaseInventoryRequest
    //{
    //    public int Quantity { get; set; }
    //}
}