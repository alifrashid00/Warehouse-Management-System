using StockFlow.Domain.Categories;
using StockFlow.Domain.Common;

namespace StockFlow.Domain.Products;

public class Product : BaseEntity
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string Unit { get; set; } = string.Empty;
    public int ReorderLevel { get; set; }
    public decimal UnitCost { get; set; }
    public bool IsActive { get; set; } = true;
}
