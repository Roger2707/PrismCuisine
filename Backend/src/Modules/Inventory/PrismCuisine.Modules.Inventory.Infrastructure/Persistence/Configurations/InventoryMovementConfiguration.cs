using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismCuisine.BuildingBlocks.Domain.Modules;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class InventoryMovementConfiguration : EntityConfiguration<InventoryMovement>
{
    public override void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        base.Configure(builder);

        builder.ToTable("InventoryMovements", ModuleSchemas.Inventory);

        builder.Property(m => m.InventoryBalanceId).IsRequired();
        builder.Property(m => m.InventoryCostLayerId).IsRequired();

        builder.Property(m => m.MovementType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(m => m.Quantity).HasPrecision(18, 4).IsRequired();
        builder.Property(m => m.UnitCost).HasPrecision(18, 4).IsRequired();

        builder.Property(m => m.ReferenceType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(m => m.Reference).HasMaxLength(128);
        builder.Property(m => m.Notes).HasMaxLength(500);

        builder.HasIndex(m => m.InventoryBalanceId);
        builder.HasIndex(m => m.InventoryCostLayerId);
        builder.HasIndex(m => new { m.ReferenceType, m.ReferenceId });
        builder.HasIndex(m => new { m.ReferenceType, m.Reference, m.MovementType });
    }
}
