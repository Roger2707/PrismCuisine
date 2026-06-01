using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismCuisine.BuildingBlocks.Domain.Modules;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Purchasing.Domain.Entities;

namespace PrismCuisine.Modules.Purchasing.Infrastructure.Persistence.Configurations;

public sealed class GoodsReceiptLineConfiguration : EntityConfiguration<GoodsReceiptLine>
{
    public override void Configure(EntityTypeBuilder<GoodsReceiptLine> builder)
    {
        base.Configure(builder);

        builder.ToTable("GoodsReceiptLines", ModuleSchemas.Purchasing);

        builder.Property(l => l.GoodsReceiptId).IsRequired();
        builder.Property(l => l.PurchaseOrderLineId).IsRequired();
        builder.Property(l => l.ProductId).IsRequired();
        builder.Property(l => l.Quantity).HasPrecision(18, 4).IsRequired();
        builder.Property(l => l.UnitCost).HasPrecision(18, 4).IsRequired();
    }
}
