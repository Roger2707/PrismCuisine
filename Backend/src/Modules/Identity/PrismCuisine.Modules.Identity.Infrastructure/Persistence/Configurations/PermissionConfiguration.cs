using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismCuisine.BuildingBlocks.Domain.Modules;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Identity.Domain.Entities;

namespace PrismCuisine.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : EntityConfiguration<Permission>
{
    public override void Configure(EntityTypeBuilder<Permission> builder)
    {
        base.Configure(builder);

        builder.ToTable("Permissions", ModuleSchemas.Identity);

        builder.Property(p => p.Code)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(300)
            .IsRequired();

        builder.HasIndex(p => p.Code)
            .IsUnique();
    }
}
