using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismCuisine.BuildingBlocks.Domain.Modules;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Purchasing.Domain.Entities;

namespace PrismCuisine.Modules.Purchasing.Infrastructure.Persistence.Configurations;

public sealed class GoodsReceiptConfiguration : EntityConfiguration<GoodsReceipt>
{
    public override void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        base.Configure(builder);

        builder.ToTable("GoodsReceipts", ModuleSchemas.Purchasing);

        builder.Property(r => r.ReceiptNumber).HasMaxLength(64).IsRequired();
        builder.HasIndex(r => r.ReceiptNumber).IsUnique();

        builder.Property(r => r.PurchaseOrderId).IsRequired();
        builder.HasIndex(r => r.PurchaseOrderId);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(r => r.PostedAt);
        builder.Property(r => r.Notes).HasMaxLength(1000);

        builder.HasMany(r => r.Lines)
            .WithOne()
            .HasForeignKey(l => l.GoodsReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(GoodsReceipt.Lines))!
            .SetField("_lines");
    }
}
