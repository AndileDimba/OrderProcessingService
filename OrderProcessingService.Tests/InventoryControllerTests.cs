using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderProcessingService.Controllers;
using OrderProcessingService.DTOs;
using OrderProcessingService.Models;
using OrderProcessingService.Repository;
using System.Threading.Tasks;
using Xunit;

namespace OrderProcessingService.Tests
{
    public class InventoryControllerTests
    {
        [Fact]
        public async Task GetProductAvailability_ReturnsOk_ForExistingProduct()
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();
            var mockLogger = new Mock<ILogger<InventoryController>>();

            var productId = "PROD001";
            var response = new ProductAvailability
            {
                ProductId = productId,
                AvailableQuantity = 10,
                ReservedQuantity = 0
            };

            mockInventoryService
                .Setup(service => service.GetProductAvailabilityAsync(productId))
                .ReturnsAsync(response);

            var controller = new InventoryController(mockInventoryService.Object, mockLogger.Object);

            // Act
            var result = await controller.GetProductAvailability(productId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResponse = Assert.IsType<ProductAvailability>(okResult.Value);
            Assert.Equal(productId, actualResponse.ProductId);
            Assert.Equal(10, actualResponse.AvailableQuantity);
            Assert.Equal(0, actualResponse.ReservedQuantity);
        }

        [Fact]
        public async Task GetProductAvailability_ReturnsNotFound_ForNonExistentProduct()
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();
            var mockLogger = new Mock<ILogger<InventoryController>>();

            var productId = "PROD999";

            mockInventoryService
                .Setup(service => service.GetProductAvailabilityAsync(productId))
                .ReturnsAsync((ProductAvailability?)null);

            var controller = new InventoryController(mockInventoryService.Object, mockLogger.Object);

            // Act
            var result = await controller.GetProductAvailability(productId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ReserveItems_Succeeds_WhenEnoughAvailable()
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();
            var mockLogger = new Mock<ILogger<InventoryController>>();

            var productId = "PROD001";
            var request = new ReserveInventoryRequest { Quantity = 5 };
            var response = new ProductAvailability
            {
                ProductId = productId,
                AvailableQuantity = 5,
                ReservedQuantity = 5
            };

            mockInventoryService
                .Setup(service => service.ReserveItemsAsync(productId, request.Quantity))
                .ReturnsAsync((true, null, response));

            var controller = new InventoryController(mockInventoryService.Object, mockLogger.Object);

            // Act
            var result = await controller.ReserveItems(productId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResponse = Assert.IsType<ProductAvailability>(okResult.Value);
            Assert.Equal(5, actualResponse.AvailableQuantity);
            Assert.Equal(5, actualResponse.ReservedQuantity);
        }

        [Fact]
        public async Task ReserveItems_ReturnsBadRequest_WhenNotEnoughAvailable()
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();
            var mockLogger = new Mock<ILogger<InventoryController>>();

            var productId = "PROD001";
            var request = new ReserveInventoryRequest { Quantity = 20 };

            mockInventoryService
                .Setup(service => service.ReserveItemsAsync(productId, request.Quantity))
                .ReturnsAsync((false, "Insufficient available quantity.", null));

            var controller = new InventoryController(mockInventoryService.Object, mockLogger.Object);

            // Act
            var result = await controller.ReserveItems(productId, request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var actualResponse = Assert.IsType<Dictionary<string, string>>(badRequest.Value);

            // Check if the dictionary contains the "error" key
            Assert.True(actualResponse.ContainsKey("error"));
            Assert.Equal("Insufficient available quantity.", actualResponse["error"]);
        }

        [Fact]
        public async Task ReleaseItems_Succeeds_WhenEnoughReserved()
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();
            var mockLogger = new Mock<ILogger<InventoryController>>();

            var productId = "PROD001";
            var request = new ReleaseInventoryRequest { Quantity = 3 };
            var response = new ProductAvailability
            {
                ProductId = productId,
                AvailableQuantity = 8,
                ReservedQuantity = 2
            };

            mockInventoryService
                .Setup(service => service.ReleaseItemsAsync(productId, request.Quantity))
                .ReturnsAsync((true, null, response));

            var controller = new InventoryController(mockInventoryService.Object, mockLogger.Object);

            // Act
            var result = await controller.ReleaseItems(productId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResponse = Assert.IsType<ProductAvailability>(okResult.Value);
            Assert.Equal(8, actualResponse.AvailableQuantity);
            Assert.Equal(2, actualResponse.ReservedQuantity);
        }

        [Fact]
        public async Task ReleaseItems_ReturnsBadRequest_WhenNotEnoughReserved()
        {
            // Arrange
            var mockInventoryService = new Mock<IInventoryService>();
            var mockLogger = new Mock<ILogger<InventoryController>>();

            var productId = "PROD001";
            var request = new ReleaseInventoryRequest { Quantity = 10 };

            mockInventoryService
                .Setup(service => service.ReleaseItemsAsync(productId, request.Quantity))
                .ReturnsAsync((false, "Insufficient reserved quantity to release.", null));

            var controller = new InventoryController(mockInventoryService.Object, mockLogger.Object);

            // Act
            var result = await controller.ReleaseItems(productId, request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var actualResponse = Assert.IsType<Dictionary<string, string>>(badRequest.Value);

            // Check if the dictionary contains the "error" key
            Assert.True(actualResponse.ContainsKey("error"));
            Assert.Equal("Insufficient reserved quantity to release.", actualResponse["error"]);
        }
    }
}