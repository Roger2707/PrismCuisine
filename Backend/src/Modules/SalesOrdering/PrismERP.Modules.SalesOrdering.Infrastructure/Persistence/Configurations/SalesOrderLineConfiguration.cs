using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismERP.BuildingBlocks.Domain.Modules;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.SalesOrdering.Domain.Entities;

namespace PrismERP.Modules.SalesOrdering.Infrastructure.Persistence.Configurations;

public sealed class SalesOrderLineConfiguration : EntityConfiguration<SalesOrderLine>
{
    public override void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        base.Configure(builder);

        builder.ToTable("SalesOrderLines", ModuleSchemas.SalesOrder);

        builder.Property(l => l.SalesOrderId)
            .IsRequired();

        builder.Property(l => l.ProductId)
            .IsRequired();

        builder.Property(l => l.ProductName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(l => l.QuantityOrdered)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(l => l.UnitPrice)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(l => l.DiscountPercent)
            .HasPrecision(5, 2);
        builder.Property(l => l.VATRate)
            .HasPrecision(5, 2);

        builder.Property(l => l.DiscountAmount)
            .HasPrecision(18, 2);
        builder.Property(l => l.VATAmount)
            .HasPrecision(18, 2);
        builder.Property(l => l.LineTotal)
            .HasPrecision(18, 2);

        builder.Property(l => l.QuantityDelivered)
            .HasPrecision(18, 4);

        builder.HasIndex(l => new { l.SalesOrderId, l.ProductId })
            .IsUnique();
    }
}
