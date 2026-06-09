using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismERP.BuildingBlocks.Domain.Modules;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Purchasing.Domain.Entities;

namespace PrismERP.Modules.Purchasing.Infrastructure.Persistence.Configurations;

public sealed class SupplierConfiguration : EntityConfiguration<Supplier>
{
    public override void Configure(EntityTypeBuilder<Supplier> builder)
    {
        base.Configure(builder);

        builder.ToTable("Suppliers", ModuleSchemas.Purchasing);

        builder.Property(s => s.Code).HasMaxLength(32).IsRequired();
        builder.HasIndex(s => s.Code).IsUnique();

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Phone).HasMaxLength(32);
        builder.Property(s => s.Email).HasMaxLength(256);
        builder.Property(s => s.Address).HasMaxLength(500);
        builder.Property(s => s.TaxCode).HasMaxLength(32);
        builder.Property(s => s.IsActive).IsRequired();
    }
}
