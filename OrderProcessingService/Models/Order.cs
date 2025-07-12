using System.ComponentModel.DataAnnotations;

namespace OrderProcessingService.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        public List<OrderItem> Items { get; set; } = new();

        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class OrderItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string ProductId { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        public Guid OrderId { get; set; }
    }

    public enum OrderStatus
    {
        Pending,
        Completed,
        Cancelled,
        Shipped
    }
}