namespace StockFlow.Application.Inventory.Dtos;

public sealed record StockLevelResponse(
    int ProductId,
    string Sku,
    string Name,
    string Category,
    string Unit,
    int Quantity,
    int ReorderLevel,
    decimal UnitCost,
    bool IsLow);
