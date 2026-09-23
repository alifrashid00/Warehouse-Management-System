using StockFlow.Domain.Common;
using StockFlow.Domain.Products;

namespace StockFlow.Domain.Inventory;

public class StockItem : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
}
