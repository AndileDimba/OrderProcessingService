using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderProcessingService.Controllers;
using OrderProcessingService.Models;
using OrderProcessingService.Repository;
using Xunit;

namespace OrderProcessingService.Tests
{
    public class PaymentControllerTests
    {
        [Fact]
        public async Task ProcessPayment_ReturnsSuccess()
        {
            // Arrange
            var mockPaymentService = new Mock<IPaymentService>();
            var request = new PaymentRequest
            {
                OrderId = Guid.NewGuid(),
                Amount = 100.0m,
                PaymentMethod = "CreditCard"
            };

            var response = new PaymentResponse
            {
                TransactionId = Guid.NewGuid(),
                Status = "Completed",
                Message = "Payment processed successfully."
            };

            mockPaymentService
                .Setup(service => service.ProcessPaymentAsync(request))
                .ReturnsAsync((response, true));

            var controller = new PaymentController(mockPaymentService.Object);

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResponse = Assert.IsType<PaymentResponse>(okResult.Value);
            Assert.Equal("Completed", actualResponse.Status);
            Assert.NotEqual(Guid.Empty, actualResponse.TransactionId);
        }

        [Fact]
        public async Task GetPaymentStatus_ReturnsPaymentDetails()
        {
            // Arrange
            var mockPaymentService = new Mock<IPaymentService>();
            var transactionId = Guid.NewGuid();
            var response = new PaymentResponse
            {
                TransactionId = transactionId,
                Status = "Completed",
                Message = "Payment status retrieved successfully."
            };

            mockPaymentService
                .Setup(service => service.GetPaymentStatusAsync(transactionId))
                .ReturnsAsync(response);

            var controller = new PaymentController(mockPaymentService.Object);

            // Act
            var result = await controller.GetPaymentStatus(transactionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResponse = Assert.IsType<PaymentResponse>(okResult.Value);
            Assert.Equal(transactionId, actualResponse.TransactionId);
            Assert.Equal("Completed", actualResponse.Status);
        }

        [Fact]
        public async Task ProcessPayment_ReturnsBadRequest_WhenOrderIdIsInvalid()
        {
            // Arrange
            var mockPaymentService = new Mock<IPaymentService>();
            var request = new PaymentRequest
            {
                OrderId = Guid.NewGuid(), // Non-existent OrderId
                Amount = 100.0m,
                PaymentMethod = "CreditCard"
            };

            var response = new PaymentResponse
            {
                Status = "Failed",
                Message = "Order with ID does not exist."
            };

            mockPaymentService
                .Setup(service => service.ProcessPaymentAsync(request))
                .ReturnsAsync((response, false));

            var controller = new PaymentController(mockPaymentService.Object);

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var actualResponse = Assert.IsType<PaymentResponse>(badRequest.Value);
            Assert.Equal("Failed", actualResponse.Status);
            Assert.Contains("does not exist", actualResponse.Message);
        }

        [Fact]
        public async Task ProcessPayment_CanSimulateFailure()
        {
            // Arrange
            var mockPaymentService = new Mock<IPaymentService>();
            var request = new PaymentRequest
            {
                OrderId = Guid.NewGuid(),
                Amount = 100.0m,
                PaymentMethod = "CreditCard"
            };

            var response = new PaymentResponse
            {
                TransactionId = Guid.NewGuid(),
                Status = "Failed",
                Message = "Payment failed. Please try again."
            };

            mockPaymentService
                .Setup(service => service.ProcessPaymentAsync(request))
                .ReturnsAsync((response, false));

            var controller = new PaymentController(mockPaymentService.Object);

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var actualResponse = Assert.IsType<PaymentResponse>(badRequest.Value);
            Assert.Equal("Failed", actualResponse.Status);
            Assert.Contains("Payment failed", actualResponse.Message);
        }

        [Fact]
        public async Task GetPaymentStatus_ReturnsNotFound_WhenTransactionDoesNotExist()
        {
            // Arrange
            var mockPaymentService = new Mock<IPaymentService>();
            var transactionId = Guid.NewGuid();

            mockPaymentService
                .Setup(service => service.GetPaymentStatusAsync(transactionId))
                .ReturnsAsync((PaymentResponse?)null);

            var controller = new PaymentController(mockPaymentService.Object);

            // Act
            var result = await controller.GetPaymentStatus(transactionId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);

            Assert.Contains("Payment transaction not found", notFoundResult.Value.ToString());
        }
    }
}