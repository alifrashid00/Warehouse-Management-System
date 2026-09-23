using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Categories;
using StockFlow.Domain.Categories;

namespace StockFlow.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly StockFlowDbContext _dbContext;

    public CategoryRepository(StockFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Category> AddAsync(Category category, CancellationToken cancellationToken)
    {
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
