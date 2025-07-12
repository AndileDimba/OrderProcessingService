using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Data;
using OrderProcessingService.Models;
using OrderProcessingService.Repository;

namespace OrderProcessingService.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderProcessingContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(OrderProcessingContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, string? ErrorMessage, Order? CreatedOrder)> CreateOrderAsync(Order order)
        {
            _logger.LogInformation("CreateOrderAsync called with TotalAmount: {TotalAmount} and ItemCount: {ItemCount}",
        order.TotalAmount, order.Items?.Count ?? 0);
            // Validate order items
            if (order.Items == null || !order.Items.Any())
            {
                _logger.LogWarning("Order validation failed: No items in the order.");
                return (false, "Order must contain at least one item.", null);
            }

            if (order.Items.Any(i => i.Quantity <= 0 || i.UnitPrice <= 0))
            {
                _logger.LogWarning("Order validation failed: Invalid item quantities or prices.");
                return (false, "Each item must have a positive quantity and unit price.", null);
            }

            // Validate total amount
            var calculatedTotal = order.Items.Sum(i => i.Quantity * i.UnitPrice);
            if (order.TotalAmount != calculatedTotal)
            {
                _logger.LogWarning("Order validation failed: TotalAmount mismatch. Expected: {CalculatedTotal}, Actual: {TotalAmount}",
                calculatedTotal, order.TotalAmount);
                return (false, $"TotalAmount ({order.TotalAmount}) does not match sum of items ({calculatedTotal}).", null);
            }

            // Reserve inventory
            var (success, errorMessage) = await ValidateAndReserveInventoryAsync(order);
            if (!success)
            {
                _logger.LogError("Inventory validation failed: {ErrorMessage}", errorMessage);
                return (false, errorMessage, null);
            }

            // Create the order
            order.Id = Guid.NewGuid();
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            order.Status = OrderStatus.Pending;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order created successfully with ID: {OrderId}", order.Id);
            return (true, null, order);
        }

        public async Task<(bool Success, string? ErrorMessage)> ValidateAndReserveInventoryAsync(Order order)
        {
            _logger.LogInformation("ValidateAndReserveInventoryAsync called with TotalAmount: {TotalAmount} and ItemCount: {ItemCount}",
    order.TotalAmount, order.Items.Count);

            foreach (var item in order.Items)
            {
                var inventory = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == item.ProductId);
                if (inventory == null)
                {
                    _logger.LogWarning("Product not found in inventory: {ProductId}", item.ProductId);
                    return (false, $"Product {item.ProductId} does not exist in inventory.");
                }

                if (inventory.AvailableQuantity < item.Quantity)
                {
                    _logger.LogWarning("Insufficient inventory for Product: {ProductId}. Requested: {Requested}, Available: {Available}",
                    item.ProductId, item.Quantity, inventory.AvailableQuantity);
                    return (false, $"Insufficient inventory for product {item.ProductId}.");
                }

                // Reserve inventory
                inventory.AvailableQuantity -= item.Quantity;
                inventory.ReservedQuantity += item.Quantity;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Inventory reserved successfully for Order with TotalAmount: {TotalAmount} and ItemCount: {ItemCount}",
        order.TotalAmount, order.Items?.Count ?? 0);
            return (true, null);
        }

        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetOrdersAsync(int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            return await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid id, OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return false;

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}