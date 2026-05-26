using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismCuisine.BuildingBlocks.Domain.Modules;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Identity.Domain.Entities;

namespace PrismCuisine.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : EntityConfiguration<Role>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        base.Configure(builder);

        builder.ToTable("Roles", ModuleSchemas.Identity);

        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.NormalizedName)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(r => r.NormalizedName)
            .IsUnique();
    }
}
