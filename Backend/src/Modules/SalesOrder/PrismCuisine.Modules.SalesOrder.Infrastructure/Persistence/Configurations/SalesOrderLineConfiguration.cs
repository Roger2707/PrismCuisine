using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismCuisine.BuildingBlocks.Domain.Modules;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.SalesOrder.Domain.Entities;

namespace PrismCuisine.Modules.SalesOrder.Infrastructure.Persistence.Configurations;

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

        builder.Property(l => l.Quantity)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(l => l.UnitPrice)
            .HasPrecision(18, 4)
            .IsRequired();
    }
}
