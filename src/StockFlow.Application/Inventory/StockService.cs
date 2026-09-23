using StockFlow.Application.Inventory.Dtos;

namespace StockFlow.Application.Inventory;

public class StockService
{
    public const int MaxResults = 50;

    private readonly IStockRepository _stockRepository;

    public StockService(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<List<StockLevelResponse>> GetStockLevelsAsync(string? search, bool lowOnly, CancellationToken cancellationToken)
    {
        return await _stockRepository.GetStockLevelsAsync(search, lowOnly, MaxResults, cancellationToken);
    }
}
