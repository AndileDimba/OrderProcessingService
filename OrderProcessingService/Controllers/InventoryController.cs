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
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        // GET /api/inventory/{productId} - Check product availability
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductAvailability(string productId)
        {
            _logger.LogInformation("GetProductAvailability called for ProductId: {ProductId}", productId);

            var result = await _inventoryService.GetProductAvailabilityAsync(productId);

            if (result == null)
            {
                _logger.LogWarning("Product not found in inventory: {ProductId}", productId);
                return NotFound(new { error = $"Product {productId} not found in inventory." });
            }

            _logger.LogInformation("Product availability retrieved for ProductId: {ProductId}", productId);
            return Ok(result);
        }

        // POST /api/inventory/{productId}/reserve - Reserve items
        [HttpPost("{productId}/reserve")]
        public async Task<IActionResult> ReserveItems(string productId, [FromBody] ReserveInventoryRequest request)
        {
            _logger.LogInformation("ReserveItems called for ProductId: {ProductId} with Quantity: {Quantity}", productId, request.Quantity);
            var (isSuccess, errorMessage, result) = await _inventoryService.ReserveItemsAsync(productId, request.Quantity);

            if (!isSuccess)
            {
                // Ensure errorMessage is not null
                _logger.LogWarning("Failed to reserve items for ProductId: {ProductId}. Error: {ErrorMessage}", productId, errorMessage);
                return BadRequest(new Dictionary<string, string>
        {
            { "error", errorMessage ?? "An unknown error occurred while reserving items." }});
            }

            _logger.LogInformation("Items reserved successfully for ProductId: {ProductId} with Quantity: {Quantity}", productId, request.Quantity);
            return Ok(result);
        }

        // POST /api/inventory/{productId}/release - Release reserved items
        [HttpPost("{productId}/release")]
        public async Task<IActionResult> ReleaseItems(string productId, [FromBody] ReleaseInventoryRequest request)
        {
            _logger.LogInformation("ReleaseItems called for ProductId: {ProductId} with Quantity: {Quantity}", productId, request.Quantity);

            var (isSuccess, errorMessage, result) = await _inventoryService.ReleaseItemsAsync(productId, request.Quantity);

            if (!isSuccess)
            {
                // Ensure errorMessage is not null
                _logger.LogWarning("Failed to release items for ProductId: {ProductId}. Error: {ErrorMessage}", productId, errorMessage);
                return BadRequest(new Dictionary<string, string>
        {
            { "error", errorMessage ?? "An unknown error occurred while releasing items." }
        });
            }

            _logger.LogInformation("Items released successfully for ProductId: {ProductId} with Quantity: {Quantity}", productId, request.Quantity);
            return Ok(result);
        }
    }
}