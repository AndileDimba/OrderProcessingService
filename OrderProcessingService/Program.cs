using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Data;
using OrderProcessingService.Repository;
using OrderProcessingService.Services;
using Swashbuckle.AspNetCore.Filters;
using OrderProcessingService.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// Add Entity Framework with In-Memory database
builder.Services.AddDbContext<OrderProcessingContext>(options =>
    options.UseInMemoryDatabase("OrderProcessingDb"));

// Add Swagger custom payload
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "OrderProcessingService", Version = "v1" });
    c.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

// Configure logging
builder.Logging.ClearProviders(); // Optional: Clear default providers
builder.Logging.AddConsole();    // Add console logging
builder.Logging.AddDebug();      // Add debug logging

// Load environment-specific settings
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Register strongly-typed configuration
builder.Services.Configure<PaymentSettings>(builder.Configuration.GetSection("PaymentSettings"));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderProcessingService v1");
});

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OrderProcessingContext>();
    context.Database.EnsureCreated();
}

app.Run();