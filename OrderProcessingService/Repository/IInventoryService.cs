using OrderProcessingService.DTOs;

namespace OrderProcessingService.Repository
{
    public interface IInventoryService
    {
        Task<ProductAvailability?> GetProductAvailabilityAsync(string productId);
        Task<(bool IsSuccess, string? ErrorMessage, ProductAvailability? Result)> ReserveItemsAsync(string productId, int quantity);
        Task<(bool IsSuccess, string? ErrorMessage, ProductAvailability? Result)> ReleaseItemsAsync(string productId, int quantity);
    }
}
