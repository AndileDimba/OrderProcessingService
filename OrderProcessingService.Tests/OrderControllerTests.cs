using Xunit;
using Microsoft.AspNetCore.Mvc;
using OrderProcessingService.Controllers;
using OrderProcessingService.Models;
using OrderProcessingService.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace OrderProcessingService.Tests
{
    public class OrderControllerTests
    {
        private OrderProcessingContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<OrderProcessingContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            return new OrderProcessingContext(options);
        }

        [Fact]
        public async Task CreateOrder_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var context = GetDbContext();
            var orderService = new Services.OrderService(context);
            var controller = new OrderController(context, orderService);
            var order = new Order
            {
                CustomerId = "customer1",
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = "PROD001", Quantity = 2, UnitPrice = 10.0m }
                },
                TotalAmount = 20.0m
            };

            // Act
            var result = await controller.CreateOrder(order);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedOrder = Assert.IsType<Order>(createdResult.Value);
            Assert.Equal("customer1", returnedOrder.CustomerId);
            Assert.Equal(20.0m, returnedOrder.TotalAmount);
        }
    }
}