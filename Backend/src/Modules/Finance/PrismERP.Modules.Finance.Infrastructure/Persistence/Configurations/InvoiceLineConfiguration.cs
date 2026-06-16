using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismERP.BuildingBlocks.Domain.Modules;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Finance.Domain.Entities;

namespace PrismERP.Modules.Finance.Infrastructure.Persistence.Configurations;

public sealed class InvoiceLineConfiguration : EntityConfiguration<InvoiceLine>
{
    public override void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        base.Configure(builder);

        builder.ToTable("InvoiceLines", ModuleSchemas.Finance);

        builder.Property(l => l.InvoiceId)
            .IsRequired();

        builder.HasIndex(l => l.InvoiceId);

        builder.Property(l => l.ProductId)
            .IsRequired();

        builder.Property(l => l.ProductName)
            .HasMaxLength(200);

        builder.Property(l => l.Description)
            .HasMaxLength(1000);

        builder.Property(l => l.Quantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(l => l.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(l => l.TaxRate)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(l => l.TaxAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(l => l.DiscountRate)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(l => l.DiscountAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(l => l.LineTotal)
            .HasPrecision(18, 2)
            .IsRequired();
    }
}
