using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Inventory;

namespace StockFlow.Infrastructure.Persistence.Configurations;

public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        // One stock row per product.
        builder.HasIndex(s => s.ProductId)
            .IsUnique();

        builder.HasOne(s => s.Product)
            .WithMany()
            .HasForeignKey(s => s.ProductId);
    }
}
