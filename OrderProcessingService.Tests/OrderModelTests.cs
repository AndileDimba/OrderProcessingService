using Xunit;
using System.ComponentModel.DataAnnotations;
using OrderProcessingService.Models;
using System.Linq;
using System.Collections.Generic;

namespace OrderProcessingService.Tests
{
    public class OrderModelTests
    {
        [Fact]
        public void Order_WithoutCustomerId_IsInvalid()
        {
            // Arrange
            var order = new Order
            {
                CustomerId = null!,
                Items = new List<OrderItem>(),
                TotalAmount = 0
            };

            // Act
            var context = new ValidationContext(order, null, null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(order, context, results, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Order.CustomerId)));
        }
    }
}