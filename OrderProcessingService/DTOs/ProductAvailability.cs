namespace OrderProcessingService.DTOs
{
    public class ProductAvailability
    {
        public required string ProductId { get; set; }
        public int AvailableQuantity { get; set; }
        public int ReservedQuantity { get; set; }
    }
}
