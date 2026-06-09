using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismERP.BuildingBlocks.Domain.Modules;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class InventoryReservationConfiguration : EntityConfiguration<InventoryReservation>
{
    public override void Configure(EntityTypeBuilder<InventoryReservation> builder)
    {
        base.Configure(builder);

        builder.ToTable("InventoryReservations", ModuleSchemas.Inventory);

        builder.Property(r => r.InventoryBalanceId).IsRequired();
        builder.Property(r => r.Quantity).HasPrecision(18, 4).IsRequired();
        builder.Property(r => r.FulfilledQuantity).HasPrecision(18, 4).IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(r => r.ReferenceType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(r => r.ReferenceId).IsRequired();
        builder.Property(r => r.Notes).HasMaxLength(500);

        builder.HasIndex(r => r.InventoryBalanceId);
        builder.HasIndex(r => new { r.ReferenceType, r.ReferenceId });
    }
}
