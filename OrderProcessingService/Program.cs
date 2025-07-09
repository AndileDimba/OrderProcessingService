using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Data;
using OrderProcessingService.Services;
//using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<OrderService>();

// Add Entity Framework with In-Memory database
builder.Services.AddDbContext<OrderProcessingContext>(options =>
    options.UseInMemoryDatabase("OrderProcessingDb"));

// Add Swagger custom payload
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new() { Title = "OrderProcessingService", Version = "v1" });
//    c.ExampleFilters();
//});
//builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

var app = builder.Build();
//app.UseSwagger();
//app.UseSwaggerUI();
//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("https://localhost:7024/swagger/v1/swagger.json", "OrderProcessingService v1");
//});

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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