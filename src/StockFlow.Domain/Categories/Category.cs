using StockFlow.Domain.Common;
using StockFlow.Domain.Products;

namespace StockFlow.Domain.Categories;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
