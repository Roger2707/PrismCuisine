using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismCuisine.BuildingBlocks.Domain.Modules;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.SalesOrdering.Domain.Entities;

namespace PrismCuisine.Modules.SalesOrdering.Infrastructure.Persistence.Configurations;

public sealed class DeliveryNoteConfiguration : EntityConfiguration<DeliveryNote>
{
    public override void Configure(EntityTypeBuilder<DeliveryNote> builder)
    {
        base.Configure(builder);

        builder.ToTable("DeliveryNotes", ModuleSchemas.SalesOrder);

        builder.Property(d => d.DeliveryNumber)
            .HasMaxLength(64)
            .IsRequired();
        builder.HasIndex(d => d.DeliveryNumber)
            .IsUnique();

        builder.Property(d => d.SalesOrderId).IsRequired();
        builder.HasIndex(d => d.SalesOrderId);
        builder.Property(d => d.CustomerId).IsRequired();

        builder.Property(d => d.CustomerName)
            .HasMaxLength(256)
            .IsRequired();
        builder.Property(d => d.OrderNumber)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(d => d.Notes)
            .HasMaxLength(1024);

        builder.HasMany(d => d.Lines)
            .WithOne()
            .HasForeignKey(l => l.DeliveryNoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(DeliveryNote.Lines))!
            .SetField("_lines");
    }
}
