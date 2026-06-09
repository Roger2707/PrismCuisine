using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismERP.BuildingBlocks.Domain.Modules;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class WarehouseConfiguration : EntityConfiguration<Warehouse>
{
    public override void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        base.Configure(builder);

        builder.ToTable("Warehouses", ModuleSchemas.Inventory);

        builder.Property(w => w.Code)
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(w => w.Code)
            .IsUnique();

        builder.Property(w => w.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(w => w.Location)
            .HasMaxLength(500);

        builder.Property(w => w.IsActive)
            .IsRequired();
    }
}
