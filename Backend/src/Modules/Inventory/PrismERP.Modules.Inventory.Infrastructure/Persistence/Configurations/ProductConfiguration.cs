using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismERP.BuildingBlocks.Domain.Modules;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : EntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        builder.ToTable("Products", ModuleSchemas.Inventory);

        builder.Property(p => p.CategoryId)
            .IsRequired();

        builder.HasIndex(p => p.CategoryId);

        builder.Property(p => p.Sku)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(p => p.Sku)
            .IsUnique();

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Unit)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.IsActive)
            .IsRequired();
    }
}
