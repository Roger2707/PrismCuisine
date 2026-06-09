using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismERP.BuildingBlocks.Domain.Modules;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Identity.Domain.Entities;

namespace PrismERP.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserRoleConfiguration : EntityConfiguration<UserRole>
{
    public override void Configure(EntityTypeBuilder<UserRole> builder)
    {
        base.Configure(builder);

        builder.ToTable("UserRoles", ModuleSchemas.Identity);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.RoleId).IsRequired();

        builder.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
    }
}
