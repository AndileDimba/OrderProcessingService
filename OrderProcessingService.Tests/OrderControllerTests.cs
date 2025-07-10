using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Controllers;
using OrderProcessingService.Data;
using OrderProcessingService.Models;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace OrderProcessingService.Tests
{
    public class OrderControllerTests
    {
        private OrderProcessingContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<OrderProcessingContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new OrderProcessingContext(options);

            // Seed inventory
            context.InventoryItems.Add(new InventoryItem
            {
                ProductId = "PROD001",
                AvailableQuantity = 10,
                ReservedQuantity = 0
            });

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task CreateOrder_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var context = GetDbContext(Guid.NewGuid().ToString());
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