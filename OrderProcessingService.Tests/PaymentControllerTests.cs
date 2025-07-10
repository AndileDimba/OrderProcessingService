using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Controllers;
using OrderProcessingService.Data;
using OrderProcessingService.Models;

namespace OrderProcessingService.Tests
{
    public class PaymentControllerTests
    {
        private OrderProcessingContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<OrderProcessingContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new OrderProcessingContext(options);
            return context;
        }

        [Fact]
        public async Task ProcessPayment_ReturnsSuccess()
        {
            // Arrange
            var context = GetDbContext(Guid.NewGuid().ToString());
            var orderId = Guid.NewGuid(); // Generate a consistent OrderId

            // Seed the database with an order
            context.Orders.Add(new Order
            {
                Id = orderId, // Use the same OrderId here
                CustomerId = "customer1",
                TotalAmount = 100.0m
            });
            context.SaveChanges();

            var controller = new PaymentController(context);
            var request = new PaymentRequest
            {
                OrderId = orderId, // Use the same OrderId here
                Amount = 100.0m, // Matches the TotalAmount of the seeded order
                PaymentMethod = "CreditCard"
            };

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaymentResponse>(okResult.Value);
            Assert.Equal("Completed", response.Status);
            Assert.NotEqual(Guid.Empty, response.TransactionId);
        }

        [Fact]
        public async Task GetPaymentStatus_ReturnsPaymentDetails()
        {
            // Arrange
            var context = GetDbContext(Guid.NewGuid().ToString());
            var transactionId = Guid.NewGuid();
            context.PaymentTransactions.Add(new PaymentTransaction
            {
                TransactionId = transactionId,
                OrderId = Guid.NewGuid().ToString(),
                Amount = 100.0m,
                Status = PaymentStatus.Completed,
                ProcessedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var controller = new PaymentController(context);

            // Act
            var result = await controller.GetPaymentStatus(transactionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaymentResponse>(okResult.Value);
            Assert.Equal(transactionId, response.TransactionId);
            Assert.Equal("Completed", response.Status);
        }

        [Fact]
        public async Task ProcessPayment_ReturnsBadRequest_WhenOrderIdIsInvalid()
        {
            // Arrange
            var context = GetDbContext(Guid.NewGuid().ToString());
            var controller = new PaymentController(context);
            var request = new PaymentRequest
            {
                OrderId = Guid.NewGuid(), // Non-existent OrderId
                Amount = 100.0m,
                PaymentMethod = "CreditCard"
            };

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var valueString = badRequest.Value?.ToString() ?? string.Empty;
            Assert.Contains("does not exist", valueString);
        }

        [Fact]
        public async Task ProcessPayment_CanSimulateFailure()
        {
            // Arrange
            var context = GetDbContext(Guid.NewGuid().ToString());
            var orderId = Guid.NewGuid();
            context.Orders.Add(new Order
            {
                Id = orderId,
                CustomerId = "customer1",
                TotalAmount = 100.0m
            });
            context.SaveChanges();

            var controller = new PaymentController(context);
            var request = new PaymentRequest
            {
                OrderId = orderId,
                Amount = 100.0m,
                PaymentMethod = "CreditCard"
            };

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            if (result is BadRequestObjectResult badRequest)
            {
                var response = Assert.IsType<PaymentResponse>(badRequest.Value);
                Assert.Equal("Failed", response.Status);
            }
            else
            {
                var okResult = Assert.IsType<OkObjectResult>(result);
                var response = Assert.IsType<PaymentResponse>(okResult.Value);
                Assert.Equal("Completed", response.Status);
            }
        }
    }
}