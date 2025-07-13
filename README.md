# Order Processing Service

The **Order Processing Service** is a backend API designed to handle e-commerce order processing, inventory management, and payment simulation. It provides endpoints for managing orders, checking inventory, reserving/releasing items, and processing payments.

This project is built with **ASP.NET Core** and uses an **in-memory database** for simplicity. The payment functionality is simulated, making it ideal for testing and prototyping.

## Features

- **Order Management**:
  - Create, update, and retrieve orders.
  - Update order statuses.

- **Inventory Management**:
  - Check product availability.
  - Reserve and release inventory items.

- **Payment Simulation**:
  - Process payments with simulated success/failure.
  - Retrieve payment statuses.

- **Logging**:
  - Detailed logging for debugging and monitoring.

- **In-Memory Database**:
  - Lightweight database for testing and development.
 
 ## Caching

### Overview
The application uses **in-memory caching** to improve performance and reduce database queries for frequently accessed data. Caching is implemented using the `IMemoryCache` service provided by ASP.NET Core.

### What is Cached?
1. **Inventory Availability**:
   - The availability of products in the inventory is cached when retrieved using the `GetProductAvailabilityAsync` method.
   - Cached data includes:
     - `ProductId`
     - `AvailableQuantity`
     - `ReservedQuantity`
  
3. **Payment Statuses**:
   - The Status of transaction is cached when retrieved using `GetPaymentStatusAsync` method.
   - Cached data includes:
      - `TransactionId`
      - `response`

2. **Cache Expiry**:
   - Cached inventory data expires after **5 minutes** to ensure consistency with the database.
  
     
### How Caching Works
1. **Cache Hit**:
   - If the requested data is already in the cache, it is returned directly without querying the database.

2. **Cache Miss**:
   - If the requested data is not in the cache, the database is queried, and the result is cached for future use.

3. **Cache Update**:
   - When inventory data is modified (e.g., items are reserved), the cache is updated to reflect the new state.

### Benefits of Caching
- **Improved Performance**:
  - Reduces the number of database queries for frequently accessed data.
- **Faster Response Times**:
  - Cached data is returned almost instantly, improving the user experience.
- **Scalability**:
  - Reduces database load, making the application more scalable.

### Future Improvements
- **Distributed Caching**:
  - For multi-instance deployments, consider using a distributed caching solution like **Redis** or **SQL Server caching**.
- **Custom Cache Expiry**:
  - Implement dynamic cache expiry based on the frequency of data changes.

## Technologies Used

- **ASP.NET Core 8.0**: Framework for building the API.
- **Entity Framework Core**: ORM for database operations.
- **In-Memory Database**: Used for testing and development.
- **xUnit**: Unit testing framework.
- **Moq**: Mocking library for testing.
- **Swagger**: API documentation and testing.
- **ILogger**: Logging interface for debugging and monitoring

## Setup and Installation

### Prerequisites
- .NET SDK 8.0 or later
- Visual Studio or any C# IDE
- Git (optional)

### Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/AndileDimba/OrderProcessingService.git
   cd OrderProcessingService
Restore dependencies:
bash
dotnet restore
Run the application:
bash
dotnet run
Access the API documentation:
Open your browser and navigate to https://localhost:7024/swagger (or the port your app is running on).

## API Endpoints

### **Order Endpoints**
- **POST /api/order**: Create a new order.
- **GET /api/order/{id}**: Retrieve an order by ID.
- **GET /api/orders**: - List orders (with pagination)
- **PUT /api/order/{id}/status**: Update the status of an order.

### **Inventory Endpoints**
- **GET /api/inventory/{productId}**: Check product availability.
- **POST /api/inventory/{productId}/reserve**: Reserve items.
- **POST /api/inventory/{productId}/release**: Release reserved items.

### **Payment Endpoints**
- **POST /api/payment/process**: Process a payment.
- **GET /api/payment/status/{transactionId}**: Retrieve the status of a payment.

## Simulated Payment Details

The payment functionality is simulated with a 50% chance of success. The following payment methods are supported:

- **CreditCard**
- **PayPal**
- **BankTransfer**

If an invalid payment method is provided, the API will return an error message.

## Testing

The project includes unit tests for controllers and services. To run the tests:

1. Navigate to the project directory:
   ```bash
   cd OrderProcessingService.Tests
Run the tests:
bash
dotnet test
The tests cover:

Order creation and status updates.
Inventory reservation and release.
Payment processing and status retrieval.

## Contributing

Contributions are welcome! To contribute:

1. Fork the repository.
2. Create a new branch:
   ```bash
   git checkout -b feature/your-feature-name
Commit your changes:
bash
git commit -m "Add your message here"
Push to your branch:
bash
git push origin feature/your-feature-name
Open a pull request.
Please ensure your code follows the project's coding standards and includes tests for new functionality.

## Contact Information

For questions or support, please contact:

- **Name**: Andile Dimba
- **Email**: mlamulia75@gmail.com.com
- **GitHub**: [AndileDimba](https://github.com/AndileDimba)
