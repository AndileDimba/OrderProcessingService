using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Data;
using OrderProcessingService.DTOs;
using OrderProcessingService.Repository;

namespace OrderProcessingService.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly OrderProcessingContext _context;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(OrderProcessingContext context, ILogger<InventoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ProductAvailability?> GetProductAvailabilityAsync(string productId)
        {
            _logger.LogInformation("GetProductAvailabilityAsync called for ProductId: {ProductId}", productId);

            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (item == null)
            {
                _logger.LogWarning("Product not found in inventory: {ProductId}", productId);
                return null;
            }

            _logger.LogInformation("Product availability retrieved for ProductId: {ProductId}", productId);
            return new ProductAvailability
            {
                ProductId = item.ProductId,
                AvailableQuantity = item.AvailableQuantity,
                ReservedQuantity = item.ReservedQuantity
            };
        }

        public async Task<(bool IsSuccess, string? ErrorMessage, ProductAvailability? Result)> ReserveItemsAsync(string productId, int quantity)
        {
            _logger.LogInformation("ReserveItemsAsync called for ProductId: {ProductId} with Quantity: {Quantity}", productId, quantity);

            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (item == null)
            {
                _logger.LogWarning("Product not found in inventory: {ProductId}", productId);
                return (false, $"Product {productId} not found in inventory.", null);
            }

            if (quantity <= 0)
            {
                _logger.LogWarning("Invalid quantity for ProductId: {ProductId}. Quantity: {Quantity}", productId, quantity);
                return (false, "Quantity must be greater than zero.", null);
            }

            if (item.AvailableQuantity < quantity)
            {
                _logger.LogWarning("Insufficient available quantity for ProductId: {ProductId}. Requested: {Requested}, Available: {Available}",
                    productId, quantity, item.AvailableQuantity);
                return (false, "Insufficient available quantity.", null);
            }

            item.AvailableQuantity -= quantity;
            item.ReservedQuantity += quantity;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Items reserved successfully for ProductId: {ProductId} with Quantity: {Quantity}", productId, quantity);
            return (true, null, new ProductAvailability
            {
                ProductId = item.ProductId,
                AvailableQuantity = item.AvailableQuantity,
                ReservedQuantity = item.ReservedQuantity
            });
        }

        public async Task<(bool IsSuccess, string? ErrorMessage, ProductAvailability? Result)> ReleaseItemsAsync(string productId, int quantity)
        {
            _logger.LogInformation("ReleaseItemsAsync called for ProductId: {ProductId} with Quantity: {Quantity}", productId, quantity);

            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (item == null)
            {
                _logger.LogWarning("Product not found in inventory: {ProductId}", productId);
                return (false, $"Product {productId} not found in inventory.", null);
            }

            if (quantity <= 0)
            {
                _logger.LogWarning("Invalid quantity for ProductId: {ProductId}. Quantity: {Quantity}", productId, quantity);
                return (false, "Quantity must be greater than zero.", null);
            }

            if (item.ReservedQuantity < quantity)
            {
                _logger.LogWarning("Insufficient reserved quantity for ProductId: {ProductId}. Requested: {Requested}, Reserved: {Reserved}",
                    productId, quantity, item.ReservedQuantity);
                return (false, "Insufficient reserved quantity to release.", null);
            }

            item.ReservedQuantity -= quantity;
            item.AvailableQuantity += quantity;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Items released successfully for ProductId: {ProductId} with Quantity: {Quantity}", productId, quantity);
            return (true, null, new ProductAvailability
            {
                ProductId = item.ProductId,
                AvailableQuantity = item.AvailableQuantity,
                ReservedQuantity = item.ReservedQuantity
            });
        }
    }
}
