using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismCuisine.BuildingBlocks.Domain.Modules;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using SalesOrderEntity = PrismCuisine.Modules.SalesOrder.Domain.Entities.SalesOrder;

namespace PrismCuisine.Modules.SalesOrder.Infrastructure.Persistence.Configurations;

public sealed class SalesOrderConfiguration : EntityConfiguration<SalesOrderEntity>
{
    public override void Configure(EntityTypeBuilder<SalesOrderEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("SalesOrders", ModuleSchemas.SalesOrder);

        builder.Property(o => o.OrderNumber)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasMany(o => o.Lines)
            .WithOne()
            .HasForeignKey(l => l.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(SalesOrderEntity.Lines))!
            .SetField("_lines");
    }
}
