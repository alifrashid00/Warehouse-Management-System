# Warehouse Management System (.NET Backend Portfolio Project)

## 1. Project Overview

### Project name

**StockFlow — Warehouse and Inventory Management System**

### One-line description

A production-style warehouse management backend that manages products, suppliers, purchase orders, stock movements, inventory reservations, warehouse transfers, sales orders, shipments, and operational reporting.

### Main goal

Build a serious backend project that directly demonstrates the skills requested in a junior/fresher .NET developer role:

- C# and ASP.NET Core Web API
- Clean, maintainable, testable code
- SQL Server queries and stored procedures
- Database transactions and concurrency control
- Debugging and troubleshooting
- API integration
- Performance optimization
- Security
- Background processing
- Code reviews and technical documentation
- Scalable architecture

This should not be a simple CRUD application. The strongest part of the project should be solving realistic backend problems correctly.

---

# 2. Why This Project Fits the Job Requirements

| Job requirement | How StockFlow demonstrates it |
|---|---|
| Develop scalable .NET applications | ASP.NET Core Web API with modular architecture |
| Write clean and maintainable code | Clean Architecture, SOLID, DTOs, validation, centralized error handling |
| Debug and troubleshoot | Structured logging, exception handling, request tracing, reproducible bug scenarios |
| Optimize speed and scalability | SQL indexes, pagination, Redis caching, query profiling, async APIs |
| Work with SQL Server | Relational schema, joins, transactions, indexes, views, stored procedures |
| Integrate third-party services | Email/webhook provider, shipping API mock, currency or notification service |
| Build secure applications | JWT authentication, role-based authorization, ownership checks, audit logs |
| Collaborate and discuss technical decisions | Architecture Decision Records, API documentation, pull-request-style commits |
| Stay current with .NET | Modern ASP.NET Core, EF Core, async/await, BackgroundService, health checks |
| Testing | Unit tests, integration tests, concurrency tests, API tests |

---

# 3. Project Scope

## 3.1 Main users

### Admin

- Manage users and roles
- Manage warehouses
- Manage products and categories
- Manage suppliers
- View system-wide reports
- Review audit logs

### Warehouse manager

- View warehouse inventory
- Approve purchase orders
- Receive stock
- Transfer stock between warehouses
- Review low-stock alerts
- Manage warehouse staff

### Warehouse staff

- Scan or enter stock movements
- Pick and pack orders
- Update shipment status
- View assigned warehouse tasks

### Sales/operator user

- Create sales orders
- Reserve inventory
- Check order status
- Request shipment

---

# 4. Core Business Workflow

The system should support this complete workflow:

```text
Supplier
   |
   v
Purchase Order
   |
   v
Goods Receiving
   |
   v
Warehouse Inventory
   |
   +--> Stock Transfer --> Another Warehouse
   |
   +--> Inventory Reservation
   |          |
   |          v
   |      Sales Order
   |          |
   |          v
   |      Picking/Packing
   |          |
   |          v
   |       Shipment
   |
   v
Stock Reports / Low-stock Alerts / Audit History
```

## Example scenario

1. Admin creates a product named `Wireless Keyboard`.
2. Warehouse A is created.
3. Supplier `Tech Supplier Ltd.` is added.
4. A purchase order for 100 keyboards is created.
5. Warehouse staff receive 100 keyboards.
6. Inventory increases from 0 to 100.
7. A customer order requests 30 keyboards.
8. The system reserves 30 units.
9. Available stock becomes 70.
10. Staff pick and pack the order.
11. Shipment is created.
12. When shipment is dispatched, the reserved stock is permanently deducted.
13. A stock movement record is stored for auditing.
14. If stock falls below the reorder level, a background job publishes a low-stock alert.

---

# 5. Recommended Technical Stack

## Backend

- C#
- ASP.NET Core Web API
- Entity Framework Core
- ASP.NET Core Identity or custom JWT authentication
- FluentValidation or built-in validation
- Swagger/OpenAPI
- Serilog or built-in structured logging

## Database

- Microsoft SQL Server
- Entity Framework Core migrations
- Stored procedures for reporting and performance-sensitive operations
- Indexes and foreign keys
- Transactions and concurrency control

## Supporting infrastructure

- Redis for caching
- RabbitMQ for asynchronous events
- Docker and Docker Compose
- Health checks
- Optional: Hangfire or a custom `BackgroundService`

## Testing

- xUnit
- Moq or NSubstitute
- WebApplicationFactory for integration tests
- Testcontainers for SQL Server/Redis/RabbitMQ integration tests, if practical

## Documentation

- Swagger
- README
- ER diagram
- Architecture diagram
- API examples
- Architecture Decision Records

---

# 6. Architecture

## Recommended architecture

Use a **modular monolith with Clean Architecture**.

Do not start with multiple microservices. A well-structured modular monolith is easier to build, test, debug, and explain in a fresher interview.

```text
StockFlow
│
├── API
│   ├── Controllers
│   ├── Middleware
│   ├── Authentication
│   ├── Swagger configuration
│   └── Dependency injection
│
├── Application
│   ├── DTOs
│   ├── Interfaces
│   ├── Services
│   ├── Validators
│   └── Use cases
│
├── Domain
│   ├── Entities
│   ├── Enums
│   ├── Value objects
│   └── Domain rules
│
├── Infrastructure
│   ├── EF Core DbContext
│   ├── Repositories
│   ├── SQL queries
│   ├── Redis
│   ├── RabbitMQ
│   ├── Email/shipping integrations
│   └── Background workers
│
└── Tests
    ├── UnitTests
    ├── IntegrationTests
    └── ConcurrencyTests
```

## Architectural principles

- Controllers should be thin.
- Business rules should live in application/domain services.
- Database access should not be scattered across controllers.
- Use DTOs instead of exposing EF Core entities directly.
- Keep external integrations behind interfaces.
- Use dependency injection.
- Use cancellation tokens for long-running operations.
- Use async methods for I/O operations.
- Centralize error handling.
- Validate input at the API boundary.
- Keep transactions around business operations that must be atomic.

---

# 7. Main Modules

## Module 1: Authentication and Authorization

### Features

- Login
- Password hashing
- JWT access tokens
- Role-based authorization
- User activation/deactivation
- Refresh tokens, optional
- Audit log for important actions

### Roles

```text
ADMIN
WAREHOUSE_MANAGER
WAREHOUSE_STAFF
SALES_OPERATOR
```

### Important security rules

- A warehouse staff member can only operate in assigned warehouses.
- A warehouse manager can manage assigned warehouses.
- Only admins can create or deactivate users.
- Users cannot access another user's private administrative data.
- Every sensitive stock adjustment must record the actor.

### Endpoints

```http
POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/refresh
GET    /api/users/me
GET    /api/users
PATCH  /api/users/{id}/status
```

---

## Module 2: Product and Category Management

### Features

- Create product
- Update product
- Activate/deactivate product
- Create categories
- Search products
- Filter by SKU/category/status
- Pagination and sorting
- Product reorder level
- Product unit of measurement

### Product fields

```text
Id
SKU
Name
Description
CategoryId
Unit
ReorderLevel
UnitCost
IsActive
CreatedAt
UpdatedAt
RowVersion
```

### Important rules

- SKU must be unique.
- An inactive product cannot be used in new purchase or sales orders.
- Unit cost cannot be negative.
- Product names should be searchable.
- Do not delete products that have historical stock movements. Use soft deletion or deactivation.

### Endpoints

```http
POST   /api/products
GET    /api/products
GET    /api/products/{id}
PUT    /api/products/{id}
PATCH  /api/products/{id}/status
DELETE /api/products/{id}
```

---

## Module 3: Warehouse Management

### Features

- Create warehouse
- Update warehouse
- Assign staff
- Activate/deactivate warehouse
- View warehouse capacity and stock summary

### Warehouse fields

```text
Id
Code
Name
Address
ManagerId
IsActive
CreatedAt
```

### Rules

- Warehouse code must be unique.
- An inactive warehouse cannot receive new stock.
- A warehouse must have at least one manager before processing operations.
- A staff member may be assigned to one or more warehouses.

### Endpoints

```http
POST   /api/warehouses
GET    /api/warehouses
GET    /api/warehouses/{id}
PUT    /api/warehouses/{id}
POST   /api/warehouses/{id}/staff
DELETE /api/warehouses/{id}/staff/{userId}
```

---

## Module 4: Inventory Management

This is one of the most important modules in the project.

### Inventory fields

```text
Id
WarehouseId
ProductId
QuantityOnHand
QuantityReserved
QuantityAvailable
ReorderLevel
UpdatedAt
RowVersion
```

### Inventory formula

```text
QuantityAvailable = QuantityOnHand - QuantityReserved
```

### Inventory operations

- Receive stock
- Adjust stock
- Reserve stock
- Release reservation
- Deduct stock after dispatch
- Transfer stock
- View stock history
- View low-stock products

### Endpoints

```http
GET  /api/inventory
GET  /api/inventory/warehouse/{warehouseId}
GET  /api/inventory/product/{productId}
POST /api/inventory/receive
POST /api/inventory/adjust
POST /api/inventory/reserve
POST /api/inventory/release-reservation
POST /api/inventory/deduct
GET  /api/inventory/{id}/movements
```

---

# 8. The Most Important Backend Problem: Preventing Overselling

## Problem

Suppose Warehouse A has 10 units.

Two requests arrive almost simultaneously:

```text
Request A wants to reserve 7 units.
Request B wants to reserve 6 units.
```

A bad implementation may do this:

```text
Request A reads available stock = 10
Request B reads available stock = 10

Request A decides 7 units are available
Request B decides 6 units are available

Request A updates stock
Request B updates stock

Final reserved stock = 13
Available stock = -3
```

This is a concurrency bug.

## Correct behavior

Only one request should succeed, or both should succeed only if enough stock remains.

## Recommended solution

Use a SQL Server transaction and optimistic concurrency or an atomic conditional update.

### Option A: Atomic SQL update

Conceptually:

```sql
UPDATE Inventory
SET QuantityReserved = QuantityReserved + @RequestedQuantity,
    UpdatedAt = SYSUTCDATETIME()
WHERE WarehouseId = @WarehouseId
  AND ProductId = @ProductId
  AND QuantityOnHand - QuantityReserved >= @RequestedQuantity;
```

Then check the affected row count:

```text
1 row affected  -> reservation succeeded
0 rows affected -> insufficient stock or conflicting state
```

This is often better than reading stock first and updating later.

### Option B: Transaction with row locking

Use a transaction and appropriate locking strategy when the operation involves multiple tables.

### Option C: RowVersion

Add a SQL Server `rowversion` column and let EF Core detect conflicting updates.

## What to demonstrate in the portfolio

Create a concurrency test that sends many reservation requests at the same time.

Expected result:

```text
Initial stock: 100
Concurrent requests: 20
Each request reserves: 10

Successful reservations: 10
Failed reservations: 10
Final reserved quantity: 100
Final available quantity: 0
```

The final quantity must never become negative.

This single feature can create a strong interview discussion about:

- Transactions
- Race conditions
- Isolation levels
- Atomic updates
- Optimistic concurrency
- Database constraints
- Idempotency

---

# 9. Module 5: Purchase Orders

## Features

- Create purchase order
- Add products and quantities
- Submit purchase order
- Approve/reject purchase order
- Receive partial or complete shipment
- Update inventory after receiving
- Track purchase order status

## Purchase order statuses

```text
DRAFT
SUBMITTED
APPROVED
PARTIALLY_RECEIVED
RECEIVED
CANCELLED
```

## Tables

```text
PurchaseOrders
PurchaseOrderItems
GoodsReceipts
GoodsReceiptItems
Suppliers
```

## Business rules

- A draft purchase order can be edited.
- Only authorized users can approve purchase orders.
- Received quantity cannot exceed ordered quantity.
- Receiving stock must update inventory and create a stock movement in one transaction.
- A purchase order cannot be marked received until all items are received.
- Duplicate receiving requests must not increase stock twice.

## Endpoints

```http
POST  /api/purchase-orders
GET   /api/purchase-orders
GET   /api/purchase-orders/{id}
POST  /api/purchase-orders/{id}/submit
POST  /api/purchase-orders/{id}/approve
POST  /api/purchase-orders/{id}/receive
POST  /api/purchase-orders/{id}/cancel
```

---

# 10. Module 6: Sales Orders and Inventory Reservation

## Features

- Create sales order
- Add order items
- Check inventory
- Reserve inventory
- Confirm order
- Cancel order
- Release inventory when cancelled
- Track order status

## Sales order statuses

```text
DRAFT
CONFIRMED
RESERVED
PICKING
PACKED
SHIPPED
CANCELLED
```

## Business rules

- Only active products can be ordered.
- Requested quantity must be greater than zero.
- The system must reserve inventory before confirming the order.
- If one item cannot be reserved, the complete order should fail or roll back.
- Cancelling an order should release reservations.
- Repeating the same confirmation request should not reserve stock twice.

## Idempotency

For important operations, support an idempotency key:

```http
POST /api/sales-orders/{id}/confirm
Idempotency-Key: 2a8b7e...
```

If the same request is retried, the system should return the original result instead of performing the operation again.

This demonstrates production-level API thinking.

---

# 11. Module 7: Warehouse Transfers

## Features

- Request transfer from Warehouse A to Warehouse B
- Approve transfer
- Dispatch transfer
- Receive transfer
- Track transfer status

## Transfer statuses

```text
REQUESTED
APPROVED
DISPATCHED
RECEIVED
CANCELLED
```

## Important transaction

When dispatching a transfer:

```text
1. Check source warehouse stock.
2. Deduct stock from source warehouse.
3. Create an in-transit record.
4. Create a transfer movement.
5. Commit transaction.
```

When receiving:

```text
1. Verify transfer is dispatched.
2. Add stock to destination warehouse.
3. Mark transfer received.
4. Create destination stock movement.
5. Commit transaction.
```

## Failure cases to handle

- Source has insufficient stock.
- Transfer is already dispatched.
- Transfer is received twice.
- Destination warehouse is inactive.
- Duplicate receive request.
- Network failure after database commit.

---

# 12. Module 8: Picking, Packing, and Shipment

## Features

- Create picking task
- Assign warehouse staff
- Mark items picked
- Mark order packed
- Create shipment
- Update shipment status
- Integrate with a shipping provider mock API

## Shipment statuses

```text
CREATED
PICKED
PACKED
DISPATCHED
DELIVERED
RETURNED
CANCELLED
```

## Third-party integration

Create an interface:

```csharp
public interface IShippingProvider
{
    Task<ShipmentResponse> CreateShipmentAsync(
        ShipmentRequest request,
        CancellationToken cancellationToken);
}
```

Implement:

```text
MockShippingProvider
```

The mock provider can simulate:

- Successful shipment creation
- Timeout
- Provider error
- Invalid address
- Retryable failure

This lets you demonstrate:

- HTTP client usage
- Dependency injection
- Retry logic
- Timeout handling
- External API abstraction
- Error mapping

---

# 13. Module 9: Low-Stock Alerts and Background Jobs

## Low-stock rule

A product is low in stock when:

```text
QuantityAvailable <= ReorderLevel
```

## Background workflow

```text
Scheduled worker
    |
    v
Find low-stock inventory
    |
    v
Create alert
    |
    v
Publish StockLow event to RabbitMQ
    |
    v
Notification worker
    |
    v
Send email/webhook
```

## Implementation options

### Option A: ASP.NET Core BackgroundService

Create a worker that runs every few minutes.

### Option B: Hangfire

Use Hangfire for recurring jobs and job monitoring.

### Option C: RabbitMQ consumer

Consume inventory events and process notifications asynchronously.

## Important design rule

Do not make the inventory API wait for email delivery.

Bad:

```text
Receive stock
  -> update database
  -> send email
  -> return API response
```

Better:

```text
Receive stock
  -> update database
  -> publish event
  -> return API response

Background worker
  -> consume event
  -> send email
```

---

# 14. Module 10: Reporting and SQL Server Stored Procedures

This module directly targets the SQL Server requirement.

## Reports

- Current inventory by warehouse
- Low-stock products
- Daily stock movements
- Purchase order summary
- Sales order summary
- Warehouse utilization
- Most frequently moved products
- Inventory valuation
- Daily shipment count
- Supplier purchase summary

## Suggested stored procedures

```sql
GetWarehouseInventory
GetLowStockProducts
GetDailyStockMovements
GetPurchaseOrderSummary
GetSalesOrderSummary
GetInventoryValuation
GetWarehousePerformance
GetProductMovementHistory
```

## Example stored procedure

```sql
CREATE PROCEDURE GetLowStockProducts
    @WarehouseId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        w.Id AS WarehouseId,
        w.Name AS WarehouseName,
        p.Id AS ProductId,
        p.SKU,
        p.Name AS ProductName,
        i.QuantityOnHand,
        i.QuantityReserved,
        i.QuantityOnHand - i.QuantityReserved AS QuantityAvailable,
        i.ReorderLevel
    FROM Inventory i
    INNER JOIN Warehouses w ON w.Id = i.WarehouseId
    INNER JOIN Products p ON p.Id = i.ProductId
    WHERE
        (@WarehouseId IS NULL OR i.WarehouseId = @WarehouseId)
        AND i.QuantityOnHand - i.QuantityReserved <= i.ReorderLevel
    ORDER BY QuantityAvailable ASC;
END;
```

## What to explain in the README

For each stored procedure, document:

- Why it exists
- Why it is not implemented as a simple EF query
- Which indexes support it
- Expected result
- Performance considerations

Do not put every query into a stored procedure. Use stored procedures where they add value.

---

# 15. SQL Server Database Design

## Core tables

```text
Users
Roles
UserRoles
Warehouses
WarehouseUsers
Categories
Products
Suppliers
Inventory
StockMovements
PurchaseOrders
PurchaseOrderItems
GoodsReceipts
GoodsReceiptItems
SalesOrders
SalesOrderItems
InventoryReservations
WarehouseTransfers
WarehouseTransferItems
PickingTasks
Shipments
Notifications
AuditLogs
IdempotencyRecords
```

## Important relationships

```text
Category 1 ---- * Product

Warehouse 1 ---- * Inventory
Product   1 ---- * Inventory

Product   1 ---- * StockMovement
Warehouse 1 ---- * StockMovement

PurchaseOrder 1 ---- * PurchaseOrderItem
SalesOrder    1 ---- * SalesOrderItem

SalesOrder 1 ---- * InventoryReservation

WarehouseTransfer 1 ---- * WarehouseTransferItem
```

## Constraints

Add database constraints wherever possible:

- Unique index on SKU
- Unique index on warehouse code
- Unique index on `(WarehouseId, ProductId)` in Inventory
- Check constraint for non-negative quantities
- Foreign keys
- Not-null constraints
- Indexes on frequently filtered fields

## Suggested indexes

```text
Products(SKU)
Products(CategoryId, IsActive)
Inventory(WarehouseId, ProductId)
Inventory(WarehouseId, QuantityOnHand, QuantityReserved)
StockMovements(ProductId, CreatedAt)
StockMovements(WarehouseId, CreatedAt)
SalesOrders(Status, CreatedAt)
PurchaseOrders(Status, CreatedAt)
```

Use actual query execution plans to justify indexes rather than adding indexes blindly.

---

# 16. Caching with Redis

Use Redis only where caching makes sense.

## Good cache candidates

- Product categories
- Warehouse list
- Product details
- Warehouse summary
- Frequently requested inventory dashboard
- User permissions, if carefully invalidated

## Example cache key

```text
warehouse:{warehouseId}:inventory-summary
```

## Cache strategy

```text
1. Request arrives.
2. Check Redis.
3. If found, return cached data.
4. If not found, query SQL Server.
5. Store result in Redis with expiration.
6. Return result.
```

## Cache invalidation

Invalidate relevant keys after:

- Stock receiving
- Stock adjustment
- Reservation
- Reservation release
- Transfer dispatch
- Transfer receive

## Important interview point

Caching inventory data can become dangerous if stale values are used for stock reservation.

Therefore:

- Use Redis for dashboards and read-heavy summaries.
- Use SQL Server as the source of truth for stock-changing operations.
- Never reserve stock based only on cached data.

---

# 17. RabbitMQ Event Design

## Suggested events

```text
StockReceived
StockReserved
StockReservationReleased
StockAdjusted
StockTransferred
LowStockDetected
SalesOrderConfirmed
ShipmentCreated
ShipmentDispatched
```

## Example event

```json
{
  "eventId": "uuid",
  "eventType": "StockReceived",
  "occurredAt": "2026-09-16T12:00:00Z",
  "warehouseId": 1,
  "productId": 10,
  "quantity": 100,
  "performedBy": 5
}
```

## Reliability considerations

- Use an event ID.
- Make consumers idempotent.
- Store processed event IDs if necessary.
- Retry transient failures.
- Send permanently failed messages to a dead-letter queue.
- Log correlation IDs.
- Do not publish an event before the database transaction commits.

## Better production design: Outbox pattern

Instead of directly publishing to RabbitMQ inside a database transaction:

```text
1. Update business data.
2. Insert event into Outbox table.
3. Commit SQL transaction.
4. Background worker reads Outbox.
5. Publish event to RabbitMQ.
6. Mark event as published.
```

This prevents the problem where the database commits but message publishing fails.

---

# 18. API Design

## General rules

- Use RESTful resource naming.
- Use correct HTTP status codes.
- Validate request bodies.
- Return consistent response formats.
- Use pagination for lists.
- Support filtering and sorting.
- Use `CancellationToken`.
- Use API versioning if practical.
- Document all endpoints in Swagger.

## Example response format

### Success

```json
{
  "success": true,
  "data": {
    "id": 10,
    "sku": "KB-001",
    "name": "Wireless Keyboard"
  },
  "error": null
}
```

### Error

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "INSUFFICIENT_STOCK",
    "message": "Not enough available stock for product KB-001."
  }
}
```

## Useful HTTP status codes

```text
200 OK
201 Created
204 No Content
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
422 Unprocessable Entity
500 Internal Server Error
```

Use `409 Conflict` for cases such as:

- Duplicate SKU
- Concurrent update conflict
- Insufficient stock
- Duplicate operation
- Invalid state transition

---

# 19. Error Handling and Debugging

## Central exception middleware

Create middleware that:

- Catches unhandled exceptions
- Logs exception details
- Generates a correlation ID
- Returns safe error responses
- Does not expose stack traces in production

## Error categories

```text
ValidationError
NotFoundError
UnauthorizedError
ForbiddenError
ConflictError
ExternalServiceError
DatabaseError
```

## Logging examples

Log:

- Request ID
- User ID
- Endpoint
- Operation name
- Warehouse ID
- Product ID
- Order ID
- Execution time
- Exception details
- External API response status

## Debugging scenarios to intentionally test

1. Duplicate SKU creation
2. Negative stock adjustment
3. Double reservation
4. Duplicate shipment creation
5. Failed RabbitMQ connection
6. SQL Server timeout
7. Shipping provider timeout
8. Invalid JWT
9. Unauthorized warehouse access
10. Duplicate event delivery

Document at least two bugs in the README:

```text
Bug:
Two concurrent reservations could reserve more stock than available.

Root cause:
The code read stock and updated it in separate operations.

Fix:
Replaced the read-then-write logic with an atomic conditional SQL update.

Test:
Added a concurrent reservation test.
```

This is valuable evidence of debugging ability.

---

# 20. Testing Strategy

## Unit tests

Test business rules without a real database.

Examples:

- Cannot create product with negative cost.
- Cannot receive negative quantity.
- Cannot cancel a shipped order.
- Cannot transfer to inactive warehouse.
- Cannot reserve unavailable stock.
- Cannot approve an already cancelled purchase order.

## Integration tests

Test with SQL Server or a test database.

Examples:

- Product creation persists correctly.
- Receiving stock updates inventory.
- Reservation creates a reservation record.
- Cancelling an order releases stock.
- Stored procedures return correct data.
- Authorization prevents unauthorized warehouse access.

## Concurrency tests

This is a major differentiator.

Test:

```text
Initial available stock = 100

Send 20 simultaneous requests.
Each request reserves 10 units.

Expected:
10 successful requests
10 failed requests
Final reserved stock = 100
Final available stock = 0
```

## API tests

Test:

- Status codes
- Validation errors
- Authentication
- Authorization
- Pagination
- Filtering
- Error response format

## Test naming convention

```text
ReserveStock_WhenQuantityIsAvailable_ShouldReserveSuccessfully
ReserveStock_WhenQuantityIsInsufficient_ShouldReturnConflict
ReserveStock_WhenRequestsAreConcurrent_ShouldNeverOversell
CancelOrder_WhenOrderIsReserved_ShouldReleaseInventory
CreateProduct_WhenSkuAlreadyExists_ShouldReturnConflict
```

---

# 21. Performance Optimization Plan

Do not claim that the system is optimized without measurements.

## Baseline first

Measure:

- API response time
- SQL query duration
- Number of database queries
- Memory usage
- Concurrent request behavior

## Possible optimizations

### Database

- Add indexes based on query plans.
- Use projections instead of loading full entities.
- Use `AsNoTracking()` for read-only queries.
- Avoid N+1 queries.
- Use pagination.
- Use compiled queries only when justified.
- Use stored procedures for selected reports.

### API

- Use async I/O.
- Avoid unnecessary serialization.
- Use response compression where appropriate.
- Validate input early.
- Avoid returning huge datasets.

### Caching

- Cache read-heavy summaries.
- Set expiration times.
- Invalidate cache after writes.

### Background processing

- Move notifications and reports out of request paths.
- Use bounded queues where necessary.
- Retry transient failures.

## Performance experiment

Create a report in the README:

```text
Scenario:
GET /api/inventory?warehouseId=1

Before:
Average response time: [measure]

Change:
Added composite index and projection.

After:
Average response time: [measure]

Conclusion:
Explain the measured improvement and trade-offs.
```

Use your actual measurements. Never invent benchmark numbers.

---

# 22. Security Checklist

- [ ] Passwords are hashed, never stored as plain text.
- [ ] JWT validation is enabled.
- [ ] Role-based authorization is implemented.
- [ ] Warehouse ownership/access is checked.
- [ ] Input validation is enabled.
- [ ] SQL injection is prevented through parameterized queries/EF Core.
- [ ] Sensitive data is not logged.
- [ ] Production secrets are not committed.
- [ ] CORS is configured appropriately.
- [ ] Rate limiting is considered for authentication endpoints.
- [ ] Audit logs exist for stock adjustments.
- [ ] Error responses do not expose internal details.
- [ ] Idempotency is implemented for important write operations.

---

# 23. Docker Setup

## Containers

```text
stockflow-api
sql-server
redis
rabbitmq
```

## Example Docker Compose services

```yaml
services:
  api:
    build: .
    depends_on:
      - sqlserver
      - redis
      - rabbitmq

  sqlserver:
    image: mcr.microsoft.com/mssql/server

  redis:
    image: redis

  rabbitmq:
    image: rabbitmq:management
```

## What to include

- Dockerfile
- docker-compose.yml
- Environment variable configuration
- Database migration instructions
- Seed data
- Health checks

Do not commit real passwords or API keys.

---

# 24. Suggested Folder Structure

```text
StockFlow/
├── src/
│   ├── StockFlow.Api/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Extensions/
│   │   └── Program.cs
│   │
│   ├── StockFlow.Application/
│   │   ├── Common/
│   │   ├── Products/
│   │   ├── Inventory/
│   │   ├── PurchaseOrders/
│   │   ├── SalesOrders/
│   │   ├── Warehouses/
│   │   └── Reports/
│   │
│   ├── StockFlow.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── Exceptions/
│   │   └── Events/
│   │
│   └── StockFlow.Infrastructure/
│       ├── Persistence/
│       ├── Repositories/
│       ├── Redis/
│       ├── Messaging/
│       ├── Integrations/
│       └── BackgroundJobs/
│
├── tests/
│   ├── StockFlow.UnitTests/
│   ├── StockFlow.IntegrationTests/
│   └── StockFlow.ConcurrencyTests/
│
├── docs/
│   ├── architecture.md
│   ├── database.md
│   ├── api-examples.md
│   └── decisions/
│
├── docker-compose.yml
├── Dockerfile
├── README.md
└── StockFlow.sln
```

---

# 25. Implementation Roadmap

## Phase 0: Setup

### Deliverables

- [ ] Install .NET SDK
- [ ] Create solution
- [ ] Create projects
- [ ] Configure SQL Server
- [ ] Configure EF Core
- [ ] Configure Swagger
- [ ] Configure Git
- [ ] Add `.editorconfig`
- [ ] Add `.gitignore`

### Git commit

```text
chore: initialize StockFlow solution and project structure
```

---

## Phase 1: Foundation

### Build

- [ ] Domain entities
- [ ] DbContext
- [ ] EF Core migrations
- [ ] Base entity
- [ ] Common result/error model
- [ ] Global exception middleware
- [ ] Structured logging
- [ ] Health checks

### Git commits

```text
feat: add domain entities and database context
feat: add global error handling and structured logging
feat: add health checks and API documentation
```

---

## Phase 2: Authentication and Product Management

### Build

- [ ] Login
- [ ] JWT
- [ ] Roles
- [ ] Product CRUD
- [ ] Category CRUD
- [ ] Validation
- [ ] Pagination
- [ ] Filtering

### Acceptance criteria

- User can log in.
- Unauthorized users cannot access protected endpoints.
- SKU uniqueness is enforced.
- Product list supports search and pagination.

---

## Phase 3: Warehouse and Inventory

### Build

- [ ] Warehouse CRUD
- [ ] Warehouse-user assignment
- [ ] Inventory table
- [ ] Stock receiving
- [ ] Stock adjustment
- [ ] Stock movement history
- [ ] Transaction handling
- [ ] Audit logs

### Acceptance criteria

- Receiving stock updates inventory.
- Every stock change creates a movement record.
- Negative stock is rejected.
- Stock changes are atomic.

---

## Phase 4: Purchase Orders

### Build

- [ ] Supplier CRUD
- [ ] Purchase order creation
- [ ] Approval workflow
- [ ] Goods receiving
- [ ] Partial receiving
- [ ] Inventory update

### Acceptance criteria

- Received quantity cannot exceed ordered quantity.
- Repeated receiving cannot duplicate stock.
- Purchase order status transitions are validated.

---

## Phase 5: Sales Orders and Reservations

### Build

- [ ] Sales order creation
- [ ] Inventory reservation
- [ ] Reservation release
- [ ] Order cancellation
- [ ] Idempotency
- [ ] Concurrency-safe stock updates

### Acceptance criteria

- Orders cannot reserve unavailable stock.
- Concurrent reservations never oversell.
- Cancellation releases reserved stock.
- Duplicate confirmation does not reserve twice.

---

## Phase 6: Transfers and Shipments

### Build

- [ ] Warehouse transfers
- [ ] Picking
- [ ] Packing
- [ ] Shipment creation
- [ ] Mock shipping API
- [ ] Retry and timeout handling

### Acceptance criteria

- Source stock is deducted only once.
- Destination stock is added only once.
- Shipment status transitions are validated.
- External API failures are handled safely.

---

## Phase 7: Redis, RabbitMQ, and Background Jobs

### Build

- [ ] Redis cache
- [ ] Cache invalidation
- [ ] RabbitMQ events
- [ ] Outbox table
- [ ] Background worker
- [ ] Low-stock alerts
- [ ] Dead-letter handling

### Acceptance criteria

- Read-heavy endpoints can use Redis.
- Stock writes never depend on cached stock.
- Events are not lost after database commit.
- Failed messages can be retried.

---

## Phase 8: Reporting and SQL Optimization

### Build

- [ ] Stored procedures
- [ ] Inventory reports
- [ ] Low-stock report
- [ ] Stock movement report
- [ ] Indexes
- [ ] Query execution plan analysis
- [ ] Performance measurements

### Acceptance criteria

- Reports return correct data.
- Stored procedures are documented.
- At least one query has a before/after performance comparison based on real measurements.

---

## Phase 9: Testing and Deployment

### Build

- [ ] Unit tests
- [ ] Integration tests
- [ ] Concurrency tests
- [ ] API tests
- [ ] Docker Compose
- [ ] Seed data
- [ ] CI pipeline
- [ ] README
- [ ] Architecture diagram

### Final acceptance criteria

- A new developer can run the project from the README.
- The API starts with Docker Compose.
- Swagger demonstrates the main workflows.
- Tests can be run with one command.
- Important business rules are covered by tests.
- No secrets are committed.

---

# 26. Minimum Viable Version vs Strong Portfolio Version

## Minimum viable version

Build this first:

- Authentication
- Roles
- Products
- Warehouses
- Inventory
- Purchase orders
- Sales orders
- Stock reservation
- SQL Server transactions
- Unit tests
- Swagger
- Docker

## Strong portfolio version

Add:

- Concurrency tests
- Redis
- RabbitMQ
- Outbox pattern
- Background worker
- Stored procedures
- Mock shipping API
- Idempotency
- Audit logs
- Health checks
- Performance report
- CI pipeline
- Architecture documentation

Do not start with every advanced feature. First make the core inventory workflow correct.

---

# 27. What You Should Be Able to Explain in an Interview

## C# and .NET

- Why use dependency injection?
- Difference between scoped, singleton, and transient services
- Why use async/await?
- What is middleware?
- How does model validation work?
- Why use DTOs?
- What is the difference between `IEnumerable`, `IQueryable`, and `List`?
- When should you use `AsNoTracking()`?
- How do you handle exceptions globally?

## SQL Server

- What is a transaction?
- What is a deadlock?
- What is optimistic concurrency?
- What is a rowversion column?
- Why are indexes useful?
- What is an execution plan?
- What is the difference between `INNER JOIN` and `LEFT JOIN`?
- When should you use a stored procedure?
- How do you prevent SQL injection?
- What isolation level is appropriate for stock reservation?

## Backend engineering

- How do you prevent overselling?
- How do you make an API idempotent?
- Why should notifications be asynchronous?
- What happens if RabbitMQ is unavailable?
- What happens if the database commits but event publishing fails?
- Why is Redis not the source of truth for inventory?
- How would you scale the API?
- How do you debug a slow endpoint?
- How do you protect warehouse-level access?

## System design

- How would you support multiple warehouses?
- How would you handle 10,000 concurrent orders?
- How would you track stock history?
- How would you retry failed shipping requests?
- How would you prevent duplicate messages?
- How would you design a reporting database if reports become expensive?

---

# 28. README Structure

Your README should contain:

```text
1. Project overview
2. Problem statement
3. Features
4. Architecture diagram
5. Tech stack
6. Database ER diagram
7. Setup instructions
8. Environment variables
9. Docker instructions
10. API documentation
11. Example workflows
12. Concurrency problem and solution
13. Redis caching strategy
14. RabbitMQ event flow
15. SQL stored procedures
16. Testing instructions
17. Performance measurements
18. Security considerations
19. Future improvements
```

## Include screenshots or diagrams

Recommended diagrams:

1. High-level architecture
2. Database ER diagram
3. Stock reservation sequence diagram
4. Purchase receiving sequence diagram
5. RabbitMQ/outbox flow
6. Warehouse transfer state machine

---

# 29. Example Portfolio Description

Use something similar after the project is actually implemented:

> **StockFlow — Warehouse Management System**  
> Built a modular warehouse management backend using C#, ASP.NET Core Web API, EF Core, and SQL Server. Implemented inventory receiving, stock reservations, purchase orders, warehouse transfers, and shipment workflows with transactional consistency and concurrency-safe stock updates. Added JWT-based role authorization, SQL Server stored procedures, Redis caching, RabbitMQ background events, structured logging, Docker deployment, and automated unit/integration tests.

Only mention technologies and features that you genuinely implement.

---

# 30. Recommended Build Order

Follow this exact order:

```text
1. C# fundamentals and ASP.NET Core basics
2. ASP.NET Core Web API
3. SQL Server and EF Core
4. Product and warehouse CRUD
5. Inventory and stock movements
6. Transactions
7. Purchase orders
8. Sales orders
9. Concurrency-safe reservations
10. Authentication and authorization
11. Unit and integration tests
12. Redis caching
13. RabbitMQ and background jobs
14. Stored procedures and reporting
15. Docker and deployment
16. README and interview preparation
```

## Final advice

The project becomes valuable when the implementation demonstrates engineering decisions, not when it contains the largest number of endpoints.

Prioritize these five features:

1. **Correct inventory transactions**
2. **Concurrency-safe stock reservation**
3. **SQL Server query and reporting work**
4. **Clean ASP.NET Core architecture**
5. **Testing and debugging evidence**

A smaller system that handles these correctly is more impressive than a large system full of basic CRUD endpoints.
