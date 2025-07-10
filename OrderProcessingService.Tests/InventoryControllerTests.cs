using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Controllers;
using OrderProcessingService.Data;
using OrderProcessingService.Models;
using OrderProcessingService.DTOs;
using System.Threading.Tasks;

namespace OrderProcessingService.Tests
{
    public class InventoryControllerTests
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
        public async Task GetProductAvailability_ReturnsOk_ForExistingProduct()
        {
            var context = GetDbContext(Guid.NewGuid().ToString());
            var controller = new InventoryController(context);

            var result = await controller.GetProductAvailability("PROD001");

            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic data = Assert.IsType<ProductAvailability>(okResult.Value);
            Assert.Equal("PROD001", (string)data.ProductId);
            Assert.Equal(10, (int)data.AvailableQuantity);
        }

        [Fact]
        public async Task GetProductAvailability_ReturnsNotFound_ForNonExistentProduct()
        {
            var context = GetDbContext(Guid.NewGuid().ToString());
            var controller = new InventoryController(context);

            var result = await controller.GetProductAvailability("PROD999");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ReserveItems_Succeeds_WhenEnoughAvailable()
        {
            var context = GetDbContext(Guid.NewGuid().ToString());
            var controller = new InventoryController(context);

            var request = new ReserveInventoryRequest { Quantity = 5 };
            var result = await controller.ReserveItems("PROD001", request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic data = Assert.IsType<ProductAvailability>(okResult.Value);
            Assert.Equal(5, (int)data.AvailableQuantity);
            Assert.Equal(5, (int)data.ReservedQuantity);
        }

        [Fact]
        public async Task ReserveItems_ReturnsBadRequest_WhenNotEnoughAvailable()
        {
            var context = GetDbContext(Guid.NewGuid().ToString());
            var controller = new InventoryController(context);

            var request = new ReserveInventoryRequest { Quantity = 20 };
            var result = await controller.ReserveItems("PROD001", request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ReleaseItems_Succeeds_WhenEnoughReserved()
        {
            var context = GetDbContext(Guid.NewGuid().ToString());
            var controller = new InventoryController(context);

            // Reserve first
            await controller.ReserveItems("PROD001", new ReserveInventoryRequest { Quantity = 5 });

            // Now release
            var releaseRequest = new ReleaseInventoryRequest { Quantity = 3 };
            var result = await controller.ReleaseItems("PROD001", releaseRequest);

            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic data = Assert.IsType<ProductAvailability>(okResult.Value);
            Assert.Equal(8, (int)data.AvailableQuantity);
            Assert.Equal(2, (int)data.ReservedQuantity);
        }

        [Fact]
        public async Task ReleaseItems_ReturnsBadRequest_WhenNotEnoughReserved()
        {
            var context = GetDbContext(Guid.NewGuid().ToString());
            var controller = new InventoryController(context);

            // Try to release more than reserved
            var releaseRequest = new ReleaseInventoryRequest { Quantity = 1 };
            var result = await controller.ReleaseItems("PROD001", releaseRequest);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}