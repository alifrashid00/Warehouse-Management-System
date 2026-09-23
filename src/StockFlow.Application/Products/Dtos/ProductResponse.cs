namespace StockFlow.Application.Products.Dtos;

public sealed record ProductResponse(
    int Id,
    string Sku,
    string Name,
    string? Description,
    int CategoryId,
    string Unit,
    int ReorderLevel,
    decimal UnitCost,
    bool IsActive);
