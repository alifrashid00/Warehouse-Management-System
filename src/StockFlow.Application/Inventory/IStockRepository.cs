using StockFlow.Application.Inventory.Dtos;

namespace StockFlow.Application.Inventory;

public interface IStockRepository
{
    Task<List<StockLevelResponse>> GetStockLevelsAsync(string? search, bool lowOnly, int limit, CancellationToken cancellationToken);
}
