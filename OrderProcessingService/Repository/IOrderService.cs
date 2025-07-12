using OrderProcessingService.Models;

namespace OrderProcessingService.Repository
{
    public interface IOrderService
    {
        Task<(bool Success, string? ErrorMessage, Order? CreatedOrder)> CreateOrderAsync(Order order);
        Task<(bool Success, string? ErrorMessage)> ValidateAndReserveInventoryAsync(Order order);
        Task<Order?> GetOrderByIdAsync(Guid id);
        Task<List<Order>> GetOrdersAsync(int page, int pageSize);
        Task<bool> UpdateOrderStatusAsync(Guid id, OrderStatus newStatus);
    }
}
