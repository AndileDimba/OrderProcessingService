using System.ComponentModel.DataAnnotations;

namespace OrderProcessingService.DTOs
{
    public class ReserveInventoryRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }
    }
}
