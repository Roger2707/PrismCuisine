using PrismERP.BuildingBlocks.Domain.Aggregates;
using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Finance.Domain.Enums;

namespace PrismERP.Modules.Finance.Domain.Entities;

public sealed class Invoice : AggregateRoot
{
    public string InvoiceNumber { get; private set; } = null!;
    public InvoiceType InvoiceType { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTime InvoiceDate { get; private set; }
    public DateTime? DueDate { get; private set; }
    public string? CounterpartyName { get; private set; }
    public string? CounterpartyAddress { get; private set; }
    public int? SalesOrderId { get; private set; }
    public int? DeliveryNoteId { get; private set; }
    public int? PurchaseOrderId { get; private set; }
    public int? GoodsReceiptId { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public string? Notes { get; private set; }
    public ICollection<InvoiceLine> Lines { get; private set; } = new List<InvoiceLine>();
    public ICollection<Payment> Payments { get; private set; } = new List<Payment>();

    private Invoice()
    {
    }

    public static Invoice Create(
        string invoiceNumber,
        InvoiceType invoiceType,
        DateTime invoiceDate,
        DateTime? dueDate = null,
        string? counterpartyName = null,
        string? counterpartyAddress = null,
        int? salesOrderId = null,
        int? deliveryNoteId = null,
        int? purchaseOrderId = null,
        int? goodsReceiptId = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
        {
            throw new ValidationException("invoiceNumber", "Invoice number is required.");
        }

        return new Invoice
        {
            InvoiceNumber = invoiceNumber.Trim().ToUpperInvariant(),
            InvoiceType = invoiceType,
            Status = InvoiceStatus.Unpaid,
            InvoiceDate = invoiceDate,
            DueDate = dueDate,
            CounterpartyName = counterpartyName?.Trim(),
            CounterpartyAddress = counterpartyAddress?.Trim(),
            SalesOrderId = salesOrderId,
            DeliveryNoteId = deliveryNoteId,
            PurchaseOrderId = purchaseOrderId,
            GoodsReceiptId = goodsReceiptId,
            Notes = notes?.Trim(),
            SubTotal = 0,
            TaxAmount = 0,
            DiscountAmount = 0,
            TotalAmount = 0,
            PaidAmount = 0
        };
    }

    public void AddLine(InvoiceLine line)
    {
        Lines.Add(line);
        RecalculateTotals();
        Touch();
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid || Status == InvoiceStatus.PartialPaid)
        {
            throw new ValidationException("status", "Cannot cancel paid or partially paid invoices.");
        }

        Status = InvoiceStatus.Cancelled;
        Touch();
    }

    public void AddPayment(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ValidationException("amount", "Payment amount must be greater than zero.");
        }

        PaidAmount += amount;
        
        if (PaidAmount >= TotalAmount)
        {
            Status = InvoiceStatus.Paid;
        }
        else
        {
            Status = InvoiceStatus.PartialPaid;
        }
        
        Touch();
    }

    public void RecalculateTotals()
    {
        SubTotal = Lines.Sum(l => l.LineTotal);
        TaxAmount = Lines.Sum(l => l.TaxAmount);
        DiscountAmount = Lines.Sum(l => l.DiscountAmount);
        TotalAmount = SubTotal + TaxAmount - DiscountAmount;
    }
}
