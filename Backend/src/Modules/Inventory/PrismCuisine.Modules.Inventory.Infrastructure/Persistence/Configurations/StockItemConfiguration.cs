using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismCuisine.BuildingBlocks.Domain.Modules;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class StockItemConfiguration : EntityConfiguration<StockItem>
{
    public override void Configure(EntityTypeBuilder<StockItem> builder)
    {
        base.Configure(builder);

        builder.ToTable("StockItems", ModuleSchemas.Inventory);

        builder.Property(s => s.ProductId)
            .IsRequired();

        builder.HasIndex(s => s.ProductId)
            .IsUnique();

        builder.Property(s => s.QuantityOnHand)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(s => s.ReorderLevel)
            .HasPrecision(18, 4)
            .IsRequired();
    }
}
