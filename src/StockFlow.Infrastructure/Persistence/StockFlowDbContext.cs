using Microsoft.EntityFrameworkCore;
using StockFlow.Domain.Categories;
using StockFlow.Domain.Inventory;
using StockFlow.Domain.Products;
using StockFlow.Domain.Users;

namespace StockFlow.Infrastructure.Persistence;

public class StockFlowDbContext : DbContext
{
    public StockFlowDbContext(DbContextOptions<StockFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>();
    public DbSet<StockItem> StockItems => Set<StockItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockFlowDbContext).Assembly);
    }
}
