using Microsoft.EntityFrameworkCore;
using StockFlow.Domain.Categories;
using StockFlow.Domain.Products;

namespace StockFlow.Infrastructure.Persistence;

public class StockFlowDbContext : DbContext
{
    public StockFlowDbContext(DbContextOptions<StockFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
}
