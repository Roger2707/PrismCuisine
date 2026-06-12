using PrismERP.BuildingBlocks.Domain.Aggregates;
using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Finance.Domain.Enums;

namespace PrismERP.Modules.Finance.Domain.Entities;

public sealed class Payment : AggregateRoot
{
    public int InvoiceId { get; private set; }
    public string PaymentNumber { get; private set; } = null!;
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentStatus Status { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? BankName { get; private set; }
    public string? AccountNumber { get; private set; }
    public string? Notes { get; private set; }

    private Payment()
    {
    }

    public static Payment Create(
        int invoiceId,
        string paymentNumber,
        PaymentMethod paymentMethod,
        decimal amount,
        DateTime paymentDate,
        string? referenceNumber = null,
        string? bankName = null,
        string? accountNumber = null,
        string? notes = null)
    {
        if (invoiceId <= 0)
        {
            throw new ValidationException("invoiceId", "Invoice ID is required.");
        }

        if (string.IsNullOrWhiteSpace(paymentNumber))
        {
            throw new ValidationException("paymentNumber", "Payment number is required.");
        }

        if (amount <= 0)
        {
            throw new ValidationException("amount", "Payment amount must be greater than zero.");
        }

        return new Payment
        {
            InvoiceId = invoiceId,
            PaymentNumber = paymentNumber.Trim().ToUpperInvariant(),
            PaymentMethod = paymentMethod,
            Status = PaymentStatus.Pending,
            Amount = amount,
            PaymentDate = paymentDate,
            ReferenceNumber = referenceNumber?.Trim(),
            BankName = bankName?.Trim(),
            AccountNumber = accountNumber?.Trim(),
            Notes = notes?.Trim()
        };
    }

    public void Complete()
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new ValidationException("status", "Only pending payments can be completed.");
        }

        Status = PaymentStatus.Completed;
        Touch();
    }

    public void Fail()
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new ValidationException("status", "Only pending payments can be failed.");
        }

        Status = PaymentStatus.Failed;
        Touch();
    }

    public void Cancel()
    {
        if (Status == PaymentStatus.Completed)
        {
            throw new ValidationException("status", "Cannot cancel completed payments.");
        }

        Status = PaymentStatus.Cancelled;
        Touch();
    }

    public void Refund()
    {
        if (Status != PaymentStatus.Completed)
        {
            throw new ValidationException("status", "Only completed payments can be refunded.");
        }

        Status = PaymentStatus.Refunded;
        Touch();
    }

    public void Update(
        PaymentMethod paymentMethod,
        string? referenceNumber,
        string? bankName,
        string? accountNumber,
        string? notes)
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new ValidationException("status", "Only pending payments can be updated.");
        }

        PaymentMethod = paymentMethod;
        ReferenceNumber = referenceNumber?.Trim();
        BankName = bankName?.Trim();
        AccountNumber = accountNumber?.Trim();
        Notes = notes?.Trim();
        Touch();
    }
}
