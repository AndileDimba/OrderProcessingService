using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Models;

namespace OrderProcessingService.Data
{
    public class OrderProcessingContext : DbContext
    {
        public OrderProcessingContext(DbContextOptions<OrderProcessingContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Order -> OrderItems relationship
            modelBuilder.Entity<OrderItem>()
                .HasOne<Order>()
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

            // Seed some initial inventory data
            modelBuilder.Entity<InventoryItem>().HasData(
                new InventoryItem { ProductId = "PROD001", AvailableQuantity = 100, ReservedQuantity = 0 },
                new InventoryItem { ProductId = "PROD002", AvailableQuantity = 50, ReservedQuantity = 0 },
                new InventoryItem { ProductId = "PROD003", AvailableQuantity = 25, ReservedQuantity = 0 }
            );
        }
    }
}