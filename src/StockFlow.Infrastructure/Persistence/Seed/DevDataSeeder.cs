using Microsoft.EntityFrameworkCore;
using StockFlow.Domain.Categories;
using StockFlow.Domain.Inventory;
using StockFlow.Domain.Products;

namespace StockFlow.Infrastructure.Persistence.Seed;

public static class DevDataSeeder
{
    public static async Task SeedAsync(StockFlowDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var seedCategories = new Dictionary<string, Category>
        {
            ["Electronics"] = new() { Name = "Electronics", Description = "Devices and accessories" },
            ["Office"] = new() { Name = "Office Supplies", Description = "Stationery and desk items" },
            ["Furniture"] = new() { Name = "Furniture", Description = "Desks, chairs and storage" },
            ["Packaging"] = new() { Name = "Packaging", Description = "Boxes, tape and wrapping" },
            ["Tools"] = new() { Name = "Tools", Description = "Hand and power tools" },
        };

        // Categories have no unique name rule yet, so duplicates can exist: keep the first of each name.
        var existingCategories = (await dbContext.Categories.ToListAsync(cancellationToken))
            .GroupBy(c => c.Name)
            .ToDictionary(g => g.Key, g => g.First());

        var categories = new Dictionary<string, Category>();
        foreach (var (key, seedCategory) in seedCategories)
        {
            if (existingCategories.TryGetValue(seedCategory.Name, out var existing))
            {
                categories[key] = existing;
            }
            else
            {
                dbContext.Categories.Add(seedCategory);
                categories[key] = seedCategory;
            }
        }

        var allProducts = new List<Product>
        {
            NewProduct("ELEC-001", "Wireless Keyboard", categories["Electronics"], "pcs", 20, 24.50m),
            NewProduct("ELEC-002", "Wireless Mouse", categories["Electronics"], "pcs", 20, 12.75m),
            NewProduct("ELEC-003", "USB-C Hub", categories["Electronics"], "pcs", 15, 31.00m),
            NewProduct("ELEC-004", "27\" Monitor", categories["Electronics"], "pcs", 5, 189.99m),
            NewProduct("ELEC-005", "Webcam 1080p", categories["Electronics"], "pcs", 10, 45.00m),
            NewProduct("OFF-001", "A4 Paper Ream", categories["Office"], "ream", 50, 4.20m),
            NewProduct("OFF-002", "Ballpoint Pens", categories["Office"], "box", 30, 6.10m),
            NewProduct("OFF-003", "Stapler", categories["Office"], "pcs", 10, 8.90m),
            NewProduct("OFF-004", "Sticky Notes", categories["Office"], "pack", 40, 2.35m),
            NewProduct("OFF-005", "Whiteboard Markers", categories["Office"], "set", 25, 5.60m),
            NewProduct("FUR-001", "Office Chair", categories["Furniture"], "pcs", 5, 129.00m),
            NewProduct("FUR-002", "Standing Desk", categories["Furniture"], "pcs", 3, 349.00m),
            NewProduct("FUR-003", "Filing Cabinet", categories["Furniture"], "pcs", 4, 96.50m),
            NewProduct("FUR-004", "Bookshelf", categories["Furniture"], "pcs", 4, 74.00m),
            NewProduct("PKG-001", "Cardboard Box (Medium)", categories["Packaging"], "pcs", 200, 0.85m),
            NewProduct("PKG-002", "Packing Tape", categories["Packaging"], "roll", 100, 1.90m),
            NewProduct("PKG-003", "Bubble Wrap", categories["Packaging"], "roll", 30, 14.00m),
            NewProduct("TOOL-001", "Cordless Drill", categories["Tools"], "pcs", 6, 79.00m),
            NewProduct("TOOL-002", "Screwdriver Set", categories["Tools"], "set", 12, 18.40m),
            NewProduct("TOOL-003", "Tape Measure", categories["Tools"], "pcs", 15, 6.75m),
        };

        var existingSkus = await dbContext.Products
            .Select(p => p.Sku)
            .ToListAsync(cancellationToken);

        dbContext.Products.AddRange(allProducts.Where(p => !existingSkus.Contains(p.Sku)));

        await dbContext.SaveChangesAsync(cancellationToken);

        await SeedStockAsync(dbContext, cancellationToken);
    }

    // Multipliers of the reorder level: 0.2x and 0.6x leave a product LOW on stock, the rest are healthy.
    private static readonly double[] StockMultipliers = [0.2, 2.0, 0.6, 3.5, 1.5];

    private static async Task SeedStockAsync(StockFlowDbContext dbContext, CancellationToken cancellationToken)
    {
        var productsWithoutStock = await dbContext.Products
            .Where(p => !dbContext.StockItems.Any(s => s.ProductId == p.Id))
            .OrderBy(p => p.Id)
            .Select(p => new { p.Id, p.ReorderLevel })
            .ToListAsync(cancellationToken);

        var stockItems = productsWithoutStock.Select((p, index) => new StockItem
        {
            ProductId = p.Id,
            Quantity = (int)Math.Round(p.ReorderLevel * StockMultipliers[index % StockMultipliers.Length]),
        });

        dbContext.StockItems.AddRange(stockItems);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Product NewProduct(string sku, string name, Category category, string unit, int reorderLevel, decimal unitCost) => new()
    {
        Sku = sku,
        Name = name,
        Category = category,
        Unit = unit,
        ReorderLevel = reorderLevel,
        UnitCost = unitCost,
    };
}
