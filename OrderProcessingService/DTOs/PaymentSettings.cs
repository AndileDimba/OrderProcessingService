namespace OrderProcessingService.DTOs
{
    public class PaymentSettings
    {
        public List<string> AllowedMethods { get; set; } = new();
    }
}
