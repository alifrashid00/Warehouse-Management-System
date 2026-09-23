using StockFlow.Domain.Products;

namespace StockFlow.Application.Products;

public interface IProductRepository
{
    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken);
    Task<Product> AddAsync(Product product, CancellationToken cancellationToken);
    Task<List<Product>> GetAllAsync(CancellationToken cancellationToken);
}
