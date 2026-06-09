using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismERP.BuildingBlocks.Domain.Modules;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class InventoryCostLayerConfiguration : EntityConfiguration<InventoryCostLayer>
{
    public override void Configure(EntityTypeBuilder<InventoryCostLayer> builder)
    {
        base.Configure(builder);

        builder.ToTable("InventoryCostLayers", ModuleSchemas.Inventory);

        builder.Property(l => l.InventoryBalanceId).IsRequired();

        builder.Property(l => l.QuantityReceived).HasPrecision(18, 4).IsRequired();
        builder.Property(l => l.QuantityRemaining).HasPrecision(18, 4).IsRequired();
        builder.Property(l => l.UnitCost).HasPrecision(18, 4).IsRequired();
        builder.Property(l => l.ReceivedAt).IsRequired();

        builder.HasIndex(l => new { l.InventoryBalanceId, l.ReceivedAt });
    }
}
