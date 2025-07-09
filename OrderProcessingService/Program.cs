using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework with In-Memory database
builder.Services.AddDbContext<OrderProcessingContext>(options =>
    options.UseInMemoryDatabase("OrderProcessingDb"));

var app = builder.Build();

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