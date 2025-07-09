using System.ComponentModel.DataAnnotations;

namespace OrderProcessingService.Models
{
    public class InventoryItem
    {
        [Key]
        public string ProductId { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int AvailableQuantity { get; set; }

        [Range(0, int.MaxValue)]
        public int ReservedQuantity { get; set; }
    }
}