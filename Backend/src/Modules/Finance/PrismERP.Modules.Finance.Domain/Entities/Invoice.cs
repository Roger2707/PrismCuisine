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
    public string? CustomerName { get; private set; }
    public string? CustomerAddress { get; private set; }
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
        string? customerName = null,
        string? customerAddress = null,
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
            Status = InvoiceStatus.Draft,
            InvoiceDate = invoiceDate,
            DueDate = dueDate,
            CustomerName = customerName?.Trim(),
            CustomerAddress = customerAddress?.Trim(),
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

    public void RemoveLine(InvoiceLine line)
    {
        Lines.Remove(line);
        RecalculateTotals();
        Touch();
    }

    public void UpdateLine(InvoiceLine line)
    {
        var existingLine = Lines.FirstOrDefault(l => l.Id == line.Id);
        if (existingLine is not null)
        {
            Lines.Remove(existingLine);
            Lines.Add(line);
            RecalculateTotals();
            Touch();
        }
    }

    public void Post()
    {
        if (Status != InvoiceStatus.Draft)
        {
            throw new ValidationException("status", "Only draft invoices can be posted.");
        }

        if (!Lines.Any())
        {
            throw new ValidationException("lines", "Invoice must have at least one line.");
        }

        Status = InvoiceStatus.Posted;
        Touch();
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid || Status == InvoiceStatus.PartiallyPaid)
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
            Status = InvoiceStatus.PartiallyPaid;
        }
        
        Touch();
    }

    private void RecalculateTotals()
    {
        SubTotal = Lines.Sum(l => l.LineTotal);
        TaxAmount = Lines.Sum(l => l.TaxAmount);
        DiscountAmount = Lines.Sum(l => l.DiscountAmount);
        TotalAmount = SubTotal + TaxAmount - DiscountAmount;
    }

    public void Update(
        DateTime? dueDate,
        string? customerName,
        string? customerAddress,
        string? notes)
    {
        if (Status != InvoiceStatus.Draft)
        {
            throw new BusinessException("Only draft invoices can be updated.");
        }

        DueDate = dueDate;
        CustomerName = customerName?.Trim();
        CustomerAddress = customerAddress?.Trim();
        Notes = notes?.Trim();
        Touch();
    }
}
