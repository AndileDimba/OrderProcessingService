using OrderProcessingService.Models;
using OrderProcessingService.Data;
using Microsoft.EntityFrameworkCore;

namespace OrderProcessingService.Services
{
    public class OrderService
    {
        private readonly OrderProcessingContext _context;

        public OrderService(OrderProcessingContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string? ErrorMessage)> ValidateAndReserveInventoryAsync(Order order)
        {
            foreach (var item in order.Items)
            {
                var inventory = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == item.ProductId);
                if (inventory == null)
                    return (false, $"Product {item.ProductId} does not exist in inventory.");

                if (inventory.AvailableQuantity < item.Quantity)
                    return (false, $"Insufficient inventory for product {item.ProductId}.");

                // Reserve inventory
                inventory.AvailableQuantity -= item.Quantity;
                inventory.ReservedQuantity += item.Quantity;
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}