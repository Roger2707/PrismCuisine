using PrismERP.BuildingBlocks.Domain.Aggregates;
using PrismERP.BuildingBlocks.Domain.Exceptions;

namespace PrismERP.Modules.Finance.Domain.Entities;

public sealed class InvoiceLine : AggregateRoot
{
    public int InvoiceId { get; private set; }
    public int ProductId { get; private set; }
    public string? ProductName { get; private set; }
    public string? Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DiscountRate { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal LineTotal { get; private set; }

    private InvoiceLine()
    {
    }

    public static InvoiceLine Create(
        int productId,
        string? productName = null,
        string? description = null,
        decimal quantity = 1,
        decimal unitPrice = 0,
        decimal taxRate = 0,
        decimal discountRate = 0)
    {
        if (quantity <= 0)
        {
            throw new ValidationException("quantity", "Quantity must be greater than zero.");
        }

        if (unitPrice < 0)
        {
            throw new ValidationException("unitPrice", "Unit price cannot be negative.");
        }

        if (taxRate < 0)
        {
            throw new ValidationException("taxRate", "Tax rate cannot be negative.");
        }

        if (discountRate < 0)
        {
            throw new ValidationException("discountRate", "Discount rate cannot be negative.");
        }

        var lineTotal = quantity * unitPrice;
        var taxAmount = lineTotal * (taxRate / 100);
        var discountAmount = lineTotal * (discountRate / 100);
        var finalTotal = lineTotal + taxAmount - discountAmount;

        return new InvoiceLine
        {
            ProductId = productId,
            ProductName = productName?.Trim(),
            Description = description?.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            TaxRate = taxRate,
            TaxAmount = taxAmount,
            DiscountRate = discountRate,
            DiscountAmount = discountAmount,
            LineTotal = finalTotal
        };
    }

    public void Update(
        string? productId,
        string? productName,
        string? description,
        decimal quantity,
        decimal unitPrice,
        decimal taxRate,
        decimal discountRate)
    {
        if (quantity <= 0)
        {
            throw new ValidationException("quantity", "Quantity must be greater than zero.");
        }

        if (unitPrice < 0)
        {
            throw new ValidationException("unitPrice", "Unit price cannot be negative.");
        }

        if (taxRate < 0)
        {
            throw new ValidationException("taxRate", "Tax rate cannot be negative.");
        }

        if (discountRate < 0)
        {
            throw new ValidationException("discountRate", "Discount rate cannot be negative.");
        }

        productId = productId?.Trim();
        ProductName = productName?.Trim();
        Description = description?.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
        DiscountRate = discountRate;

        RecalculateTotals();
        Touch();
    }

    private void RecalculateTotals()
    {
        var lineTotal = Quantity * UnitPrice;
        TaxAmount = lineTotal * (TaxRate / 100);
        DiscountAmount = lineTotal * (DiscountRate / 100);
        LineTotal = lineTotal + TaxAmount - DiscountAmount;
    }
}
