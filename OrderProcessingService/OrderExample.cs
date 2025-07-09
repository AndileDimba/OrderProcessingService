using Swashbuckle.AspNetCore.Filters;
using OrderProcessingService.Models;

public class OrderExample : IExamplesProvider<Order>
{
    public Order GetExamples()
    {
        return new Order
        {
            CustomerId = "customer123",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = "PROD001", Quantity = 2, UnitPrice = 10.5m }
            },
            TotalAmount = 21.0m
        };
    }
}