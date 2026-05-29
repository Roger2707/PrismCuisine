using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismCuisine.BuildingBlocks.Domain.Modules;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class ProductCategoryConfiguration : EntityConfiguration<ProductCategory>
{
    public override void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        base.Configure(builder);

        builder.ToTable("ProductCategories", ModuleSchemas.Inventory);

        builder.Property(c => c.Code)
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.IsActive)
            .IsRequired();
    }
}
