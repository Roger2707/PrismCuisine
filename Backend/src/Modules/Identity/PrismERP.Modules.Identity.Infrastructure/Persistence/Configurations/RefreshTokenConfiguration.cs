using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismERP.BuildingBlocks.Domain.Modules;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Identity.Domain.Entities;

namespace PrismERP.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : EntityConfiguration<RefreshToken>
{
    public override void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        base.Configure(builder);

        builder.ToTable("RefreshTokens", ModuleSchemas.Identity);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Token).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.RevokedAt);

        builder.HasIndex(x => x.UserId).IsUnique();
    }
}
