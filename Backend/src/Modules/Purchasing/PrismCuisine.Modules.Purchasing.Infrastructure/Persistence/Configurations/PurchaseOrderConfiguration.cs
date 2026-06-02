using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismCuisine.BuildingBlocks.Domain.Modules;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Purchasing.Domain.Entities;

namespace PrismCuisine.Modules.Purchasing.Infrastructure.Persistence.Configurations;

public sealed class PurchaseOrderConfiguration : EntityConfiguration<PurchaseOrder>
{
    public override void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        base.Configure(builder);

        builder.ToTable("PurchaseOrders", ModuleSchemas.Purchasing);

        builder.Property(o => o.OrderNumber)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.SupplierId).IsRequired();
        builder.Property(o => o.WarehouseId).IsRequired();
        builder.Property(o => o.AmendedFromPurchaseOrderId);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(o => o.ApprovedAt);
        builder.Property(o => o.Notes).HasMaxLength(1000);

        builder.HasMany(o => o.Lines)
            .WithOne()
            .HasForeignKey(l => l.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(PurchaseOrder.Lines))!
            .SetField("_lines");
    }
}
