using PrismCuisine.BuildingBlocks.Domain.Entities;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;

namespace PrismCuisine.Modules.SalesOrdering.Domain.Entities;

public sealed class SalesOrderLine : Entity
{
    public int SalesOrderId { get; private set; }

    // Snapshot of product details at the time of order creation
    public int ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public decimal QuantityOrdered { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal VATRate { get; private set; }

    // Computed — lưu DB để query/report nhanh
    public decimal DiscountAmount { get; private set; }
    public decimal VATAmount { get; private set; }
    public decimal LineTotal { get; private set; } // sau discount + VAT

    // Delivery tracking
    public decimal QuantityDelivered { get; private set; } = 0;
    public decimal QuantityRemaining => QuantityOrdered - QuantityDelivered;

    private SalesOrderLine()
    {
    }

    internal static SalesOrderLine Create(int productId, string productName, decimal quantityOrdered, decimal unitPrice, decimal discountPercent, decimal vatRate)
    {
        if (productId <= 0)
            throw new DomainException("ProductId is required.");

        if(string.IsNullOrWhiteSpace(productName))
            throw new DomainException("ProductName is required.");

        if (quantityOrdered <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (unitPrice <= 0)
            throw new DomainException("UnitPrice must be greater than zero.");

        if (discountPercent < 0 || discountPercent > 100)
            throw new DomainException("DiscountPercent must be between 0 and 100.");

        decimal[] validVatRates = [0, 5, 8, 10];
        if (!validVatRates.Contains(vatRate))
            throw new DomainException("VATRate must be 0, 5, 8 or 10.");

        var line = new SalesOrderLine
        {
            ProductId = productId,
            ProductName = productName.Trim(),
            QuantityOrdered = quantityOrdered,
            UnitPrice = unitPrice,
            DiscountPercent = discountPercent,
            VATRate = vatRate,
        };
        line.Calculate();
        return line;
    }

    internal void AssignToOrder(int salesOrderId) => SalesOrderId = salesOrderId;

    private void Calculate()
    {
        var gross = UnitPrice * QuantityOrdered;
        DiscountAmount = gross * (DiscountPercent / 100m);
        var afterDiscount = gross - DiscountAmount;
        VATAmount = afterDiscount * (VATRate / 100m);
        LineTotal = afterDiscount + VATAmount;
    }
}
