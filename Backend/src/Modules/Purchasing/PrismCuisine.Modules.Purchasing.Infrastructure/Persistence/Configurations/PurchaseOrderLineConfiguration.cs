using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismCuisine.BuildingBlocks.Domain.Modules;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Purchasing.Domain.Entities;

namespace PrismCuisine.Modules.Purchasing.Infrastructure.Persistence.Configurations;

public sealed class PurchaseOrderLineConfiguration : EntityConfiguration<PurchaseOrderLine>
{
    public override void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        base.Configure(builder);

        builder.ToTable("PurchaseOrderLines", ModuleSchemas.Purchasing);

        builder.Property(l => l.PurchaseOrderId)
            .IsRequired();

        builder.Property(l => l.ProductId)
            .IsRequired();

        builder.Property(l => l.Quantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(l => l.UnitPrice)
            .HasPrecision(18, 4)
            .IsRequired();
    }
}
