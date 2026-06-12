using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrismERP.BuildingBlocks.Domain.Modules;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Finance.Domain.Entities;

namespace PrismERP.Modules.Finance.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : EntityConfiguration<Payment>
{
    public override void Configure(EntityTypeBuilder<Payment> builder)
    {
        base.Configure(builder);

        builder.ToTable("Payments", ModuleSchemas.Finance);

        builder.Property(p => p.InvoiceId)
            .IsRequired();

        builder.HasIndex(p => p.InvoiceId);

        builder.Property(p => p.PaymentNumber)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(p => p.PaymentNumber)
            .IsUnique();

        builder.Property(p => p.PaymentMethod)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.PaymentDate)
            .IsRequired();

        builder.Property(p => p.ReferenceNumber)
            .HasMaxLength(64);

        builder.Property(p => p.BankName)
            .HasMaxLength(200);

        builder.Property(p => p.AccountNumber)
            .HasMaxLength(64);

        builder.Property(p => p.Notes)
            .HasMaxLength(1000);
    }
}
