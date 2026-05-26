using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismCuisine.BuildingBlocks.Domain.Modules;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Identity.Domain.Entities;

namespace PrismCuisine.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class RolePermissionConfiguration : EntityConfiguration<RolePermission>
{
    public override void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        base.Configure(builder);

        builder.ToTable("RolePermissions", ModuleSchemas.Identity);

        builder.Property(x => x.RoleId).IsRequired();
        builder.Property(x => x.PermissionId).IsRequired();

        builder.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
    }
}
