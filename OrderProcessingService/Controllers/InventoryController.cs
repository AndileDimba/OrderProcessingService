using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Data;
using OrderProcessingService.DTOs;
using OrderProcessingService.Models;
using OrderProcessingService.Repository;

namespace OrderProcessingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // GET /api/inventory/{productId} - Check product availability
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductAvailability(string productId)
        {
            var result = await _inventoryService.GetProductAvailabilityAsync(productId);
            if (result == null)
                return NotFound(new { error = $"Product {productId} not found in inventory." });

            return Ok(result);
        }

        // POST /api/inventory/{productId}/reserve - Reserve items
        [HttpPost("{productId}/reserve")]
        public async Task<IActionResult> ReserveItems(string productId, [FromBody] ReserveInventoryRequest request)
        {
            var (isSuccess, errorMessage, result) = await _inventoryService.ReserveItemsAsync(productId, request.Quantity);

            if (!isSuccess)
            {
                // Ensure errorMessage is not null
                return BadRequest(new Dictionary<string, string>
        {
            { "error", errorMessage ?? "An unknown error occurred while reserving items." }
        });
            }

            return Ok(result);
        }

        // POST /api/inventory/{productId}/release - Release reserved items
        [HttpPost("{productId}/release")]
        public async Task<IActionResult> ReleaseItems(string productId, [FromBody] ReleaseInventoryRequest request)
        {
            var (isSuccess, errorMessage, result) = await _inventoryService.ReleaseItemsAsync(productId, request.Quantity);

            if (!isSuccess)
            {
                // Ensure errorMessage is not null
                return BadRequest(new Dictionary<string, string>
        {
            { "error", errorMessage ?? "An unknown error occurred while releasing items." }
        });
            }

            return Ok(result);
        }
    }
}