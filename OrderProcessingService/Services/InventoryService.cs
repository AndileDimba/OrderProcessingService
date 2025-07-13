using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OrderProcessingService.Data;
using OrderProcessingService.DTOs;
using OrderProcessingService.Repository;

namespace OrderProcessingService.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly OrderProcessingContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(OrderProcessingContext context, IMemoryCache cache, ILogger<InventoryService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ProductAvailability?> GetProductAvailabilityAsync(string productId)
        {
            _logger.LogInformation("GetProductAvailabilityAsync called for ProductId: {ProductId}", productId);

            // Check if the product availability is already cached
            if (_cache.TryGetValue(productId, out ProductAvailability? cachedAvailability))
            {
                _logger.LogInformation("Cache hit for ProductId: {ProductId}", productId);
                return cachedAvailability;
            }

            _logger.LogInformation("Cache miss for ProductId: {ProductId}. Querying database...", productId);

            // Query the database if not cached
            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (item == null)
            {
                _logger.LogWarning("Product not found in inventory: {ProductId}", productId);
                return null;
            }

            var availability = new ProductAvailability
            {
                ProductId = item.ProductId,
                AvailableQuantity = item.AvailableQuantity,
                ReservedQuantity = item.ReservedQuantity
            };

            // Cache the result for 5 minutes
            _cache.Set(productId, availability, TimeSpan.FromMinutes(5));
            _logger.LogInformation("Cached availability for ProductId: {ProductId}", productId);

            return availability;
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

            
            var updatedAvailability =  new ProductAvailability
            {
                ProductId = item.ProductId,
                AvailableQuantity = item.AvailableQuantity,
                ReservedQuantity = item.ReservedQuantity
            };

            _cache.Set(productId, updatedAvailability, TimeSpan.FromMinutes(5));
            _logger.LogInformation("Updated cache for ProductId: {ProductId} after reserving items", productId);
            return (true, null, updatedAvailability);
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
