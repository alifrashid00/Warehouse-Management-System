using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Inventory;
using StockFlow.Application.Inventory.Dtos;

namespace StockFlow.Infrastructure.Persistence.Repositories;

public class StockRepository : IStockRepository
{
    private readonly StockFlowDbContext _dbContext;

    public StockRepository(StockFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<StockLevelResponse>> GetStockLevelsAsync(string? search, bool lowOnly, int limit, CancellationToken cancellationToken)
    {
        var query = _dbContext.StockItems.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                s.Product.Name.Contains(search) ||
                s.Product.Sku.Contains(search) ||
                s.Product.Category.Name.Contains(search));
        }

        if (lowOnly)
        {
            query = query.Where(s => s.Quantity < s.Product.ReorderLevel);
        }

        return await query
            .OrderBy(s => s.Product.Name)
            .Take(limit)
            .Select(s => new StockLevelResponse(
                s.ProductId,
                s.Product.Sku,
                s.Product.Name,
                s.Product.Category.Name,
                s.Product.Unit,
                s.Quantity,
                s.Product.ReorderLevel,
                s.Product.UnitCost,
                s.Quantity < s.Product.ReorderLevel))
            .ToListAsync(cancellationToken);
    }
}
