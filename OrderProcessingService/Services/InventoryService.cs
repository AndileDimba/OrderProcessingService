using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Data;
using OrderProcessingService.DTOs;
using OrderProcessingService.Repository;

namespace OrderProcessingService.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly OrderProcessingContext _context;

        public InventoryService(OrderProcessingContext context)
        {
            _context = context;
        }

        public async Task<ProductAvailability?> GetProductAvailabilityAsync(string productId)
        {
            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (item == null)
                return null;

            return new ProductAvailability
            {
                ProductId = item.ProductId,
                AvailableQuantity = item.AvailableQuantity,
                ReservedQuantity = item.ReservedQuantity
            };
        }

        public async Task<(bool IsSuccess, string? ErrorMessage, ProductAvailability? Result)> ReserveItemsAsync(string productId, int quantity)
        {
            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (item == null)
                return (false, $"Product {productId} not found in inventory.", null);

            if (quantity <= 0)
                return (false, "Quantity must be greater than zero.", null);

            if (item.AvailableQuantity < quantity)
                return (false, "Insufficient available quantity.", null);

            item.AvailableQuantity -= quantity;
            item.ReservedQuantity += quantity;
            await _context.SaveChangesAsync();

            return (true, null, new ProductAvailability
            {
                ProductId = item.ProductId,
                AvailableQuantity = item.AvailableQuantity,
                ReservedQuantity = item.ReservedQuantity
            });
        }

        public async Task<(bool IsSuccess, string? ErrorMessage, ProductAvailability? Result)> ReleaseItemsAsync(string productId, int quantity)
        {
            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (item == null)
                return (false, $"Product {productId} not found in inventory.", null);

            if (quantity <= 0)
                return (false, "Quantity must be greater than zero.", null);

            if (item.ReservedQuantity < quantity)
                return (false, "Insufficient reserved quantity to release.", null);

            item.ReservedQuantity -= quantity;
            item.AvailableQuantity += quantity;
            await _context.SaveChangesAsync();

            return (true, null, new ProductAvailability
            {
                ProductId = item.ProductId,
                AvailableQuantity = item.AvailableQuantity,
                ReservedQuantity = item.ReservedQuantity
            });
        }
    }
}
