using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Products;
using StockFlow.Domain.Products;

namespace StockFlow.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly StockFlowDbContext _dbContext;

    public ProductRepository(StockFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken)
    {
        return await _dbContext.Products.AnyAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
