using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismERP.BuildingBlocks.Domain.Modules;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.SalesOrdering.Domain.Entities;

namespace PrismERP.Modules.SalesOrdering.Infrastructure.Persistence.Configurations;

public sealed class DeliveryNoteLineConfiguration : EntityConfiguration<DeliveryNoteLine>
{
    public override void Configure(EntityTypeBuilder<DeliveryNoteLine> builder)
    {
        base.Configure(builder);

        builder.ToTable("DeliveryNoteLines", ModuleSchemas.SalesOrder);

        builder.Property(l => l.DeliveryNoteId).IsRequired();
        builder.Property(l => l.SalesOrderLineId).IsRequired();
        builder.Property(l => l.ProductId).IsRequired();

        builder.Property(l => l.ProductName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(l => l.QuantityDelivered)
            .HasPrecision(18, 4)
            .IsRequired();
    }
}
