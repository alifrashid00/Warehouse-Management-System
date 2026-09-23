using StockFlow.Domain.Categories;

namespace StockFlow.Application.Categories;

public interface ICategoryRepository
{
    Task<Category> AddAsync(Category category, CancellationToken cancellationToken);
    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken);
}
