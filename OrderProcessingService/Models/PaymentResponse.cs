namespace OrderProcessingService.Models
{
    public class PaymentResponse
    {
        public Guid TransactionId { get; set; }
        public string Status { get; set; } = string.Empty; // e.g., "Pending", "Completed", "Failed"
        public string? Message { get; set; } // Optional message for additional details
    }
}
