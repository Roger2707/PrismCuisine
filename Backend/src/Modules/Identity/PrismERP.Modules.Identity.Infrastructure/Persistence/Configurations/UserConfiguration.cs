using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismERP.BuildingBlocks.Domain.Modules;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Identity.Domain.Entities;

namespace PrismERP.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : EntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("Users", ModuleSchemas.Identity);

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.IsActive)
            .IsRequired();

        builder.Property(u => u.LastLoginAt);
    }
}
