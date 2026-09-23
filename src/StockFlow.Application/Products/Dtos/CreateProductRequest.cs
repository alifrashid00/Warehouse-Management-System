namespace StockFlow.Application.Products.Dtos;

public sealed record CreateProductRequest(
    string Sku,
    string Name,
    string? Description,
    int CategoryId,
    string Unit,
    int ReorderLevel,
    decimal UnitCost);
