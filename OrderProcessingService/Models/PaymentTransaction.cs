using System.ComponentModel.DataAnnotations;

namespace OrderProcessingService.Models
{
    public class PaymentTransaction
    {
        [Key]
        public Guid TransactionId { get; set; } = Guid.NewGuid();

        [Required]
        public string OrderId { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed
    }
}