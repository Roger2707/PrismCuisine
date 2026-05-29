using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismCuisine.BuildingBlocks.Domain.Modules;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class InventoryBalanceConfiguration : EntityConfiguration<InventoryBalance>
{
    public override void Configure(EntityTypeBuilder<InventoryBalance> builder)
    {
        base.Configure(builder);

        builder.ToTable("InventoryBalances", ModuleSchemas.Inventory);

        builder.Property(b => b.ProductId).IsRequired();
        builder.Property(b => b.WarehouseId).IsRequired();

        builder.HasIndex(b => new { b.ProductId, b.WarehouseId }).IsUnique();

        builder.Property(b => b.QuantityOnHand).HasPrecision(18, 4).IsRequired();
        builder.Property(b => b.ReorderLevel).HasPrecision(18, 4).IsRequired();
    }
}
