using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderProcessingService.Controllers;
using OrderProcessingService.Data;
using OrderProcessingService.DTOs;
using OrderProcessingService.Models;
using OrderProcessingService.Repository;
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
        public async Task CreateOrder_ReturnsCreated_WhenOrderIsValid()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            var mockLogger = new Mock<ILogger<OrderController>>();

            var order = new Order
            {
                Items = new List<OrderItem>
        {
            new OrderItem { ProductId = "PROD001", Quantity = 2, UnitPrice = 50 }
        },
                TotalAmount = 100
            };

            var createdOrder = new Order
            {
                Id = Guid.NewGuid(),
                Items = order.Items,
                TotalAmount = order.TotalAmount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            mockOrderService
                .Setup(service => service.CreateOrderAsync(It.Is<Order>(o => o == order)))
                .ReturnsAsync((true, null, createdOrder));

            var controller = new OrderController(mockOrderService.Object, mockLogger.Object);

            // Act
            var result = await controller.CreateOrder(order);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);

            // Check if RouteValues is not null and contains the "id" key
            Assert.NotNull(createdResult.RouteValues);
            Assert.True(createdResult.RouteValues.ContainsKey("id"));

            // Safely access the "id" value
            var routeId = createdResult.RouteValues["id"];
            Assert.NotNull(routeId);
            Assert.Equal(createdOrder.Id, routeId);

            var actualOrder = Assert.IsType<Order>(createdResult.Value);
            Assert.Equal(createdOrder.Id, actualOrder.Id);
            Assert.Equal(OrderStatus.Pending, actualOrder.Status);
        }

        [Fact]
        public async Task GetOrderById_ReturnsOrder_WhenOrderExists()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            var mockLogger = new Mock<ILogger<OrderController>>();

            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                Items = new List<OrderItem>
        {
            new OrderItem { ProductId = "PROD001", Quantity = 2, UnitPrice = 50 }
        },
                TotalAmount = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            mockOrderService
                .Setup(service => service.GetOrderByIdAsync(orderId))
                .ReturnsAsync(order);

            var controller = new OrderController(mockOrderService.Object, mockLogger.Object);

            // Act
            var result = await controller.GetOrderById(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualOrder = Assert.IsType<Order>(okResult.Value);
            Assert.Equal(orderId, actualOrder.Id);
        }

        [Fact]
        public async Task GetOrderById_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            var mockLogger = new Mock<ILogger<OrderController>>();

            var orderId = Guid.NewGuid();

            mockOrderService
                .Setup(service => service.GetOrderByIdAsync(orderId))
                .ReturnsAsync((Order?)null);

            var controller = new OrderController(mockOrderService.Object, mockLogger.Object);

            // Act
            var result = await controller.GetOrderById(orderId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetOrders_ReturnsPaginatedOrders()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            var mockLogger = new Mock<ILogger<OrderController>>();

            var orders = new List<Order>
    {
        new Order
        {
            Id = Guid.NewGuid(),
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = "PROD001", Quantity = 2, UnitPrice = 50 }
            },
            TotalAmount = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = OrderStatus.Pending
        }
    };

            mockOrderService
                .Setup(service => service.GetOrdersAsync(1, 10))
                .ReturnsAsync(orders);

            var controller = new OrderController(mockOrderService.Object, mockLogger.Object);

            // Act
            var result = await controller.GetOrders(1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualOrders = Assert.IsType<List<Order>>(okResult.Value);
            Assert.Single(actualOrders);
            Assert.Equal(orders[0].Id, actualOrders[0].Id);
        }

        [Fact]
        public async Task UpdateOrderStatus_ReturnsOk_WhenStatusIsUpdated()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            var mockLogger = new Mock<ILogger<OrderController>>();

            var orderId = Guid.NewGuid();
            var request = new UpdateOrderStatusRequest { Status = "Completed" };

            mockOrderService
                .Setup(service => service.UpdateOrderStatusAsync(orderId, OrderStatus.Completed))
                .ReturnsAsync(true);

            var controller = new OrderController(mockOrderService.Object, mockLogger.Object);

            // Act
            var result = await controller.UpdateOrderStatus(orderId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Dictionary<string, string>>(okResult.Value);
            Assert.Equal("Order status updated successfully.", response["message"]);
        }

        [Fact]
        public async Task UpdateOrderStatus_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            var mockLogger = new Mock<ILogger<OrderController>>();
            var orderId = Guid.NewGuid();
            var request = new UpdateOrderStatusRequest { Status = "Completed" };

            mockOrderService
                .Setup(service => service.UpdateOrderStatusAsync(orderId, OrderStatus.Completed))
                .ReturnsAsync(false);

            var controller = new OrderController(mockOrderService.Object, mockLogger.Object);

            // Act
            var result = await controller.UpdateOrderStatus(orderId, request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<Dictionary<string, string>>(notFoundResult.Value);
            Assert.Equal("Order not found.", response["error"]);
        }

        [Fact]
        public async Task UpdateOrderStatus_ReturnsBadRequest_WhenStatusIsInvalid()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            var mockLogger = new Mock<ILogger<OrderController>>();

            var orderId = Guid.NewGuid();
            var request = new UpdateOrderStatusRequest { Status = "InvalidStatus" };

            var controller = new OrderController(mockOrderService.Object, mockLogger.Object);

            // Act
            var result = await controller.UpdateOrderStatus(orderId, request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<Dictionary<string, string>>(badRequest.Value);
            Assert.Equal("Invalid status value.", response["error"]);
        }
    }
}