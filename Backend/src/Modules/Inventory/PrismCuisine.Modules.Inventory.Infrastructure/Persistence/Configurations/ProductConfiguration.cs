using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismCuisine.BuildingBlocks.Domain.Modules;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : EntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        builder.ToTable("Products", ModuleSchemas.Inventory);

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
    }
}
