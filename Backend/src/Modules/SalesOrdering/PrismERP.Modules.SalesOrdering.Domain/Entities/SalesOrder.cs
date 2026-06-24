using PrismERP.BuildingBlocks.Domain.Aggregates;
using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.SalesOrdering.Domain.Enums;

namespace PrismERP.Modules.SalesOrdering.Domain.Entities;

public sealed class SalesOrder : AggregateRoot
{
    private readonly List<SalesOrderLine> _lines = [];

    public string OrderNumber { get; private set; } = null!;
    public int CustomerId { get; private set; }
    public string CustomerName { get; private set; } = default!; // snapshot
    public DateTime OrderDate { get; private set; }
    public DateTime? DeliveryDate { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public SalesOrderStatus Status { get; private set; }
    public SalesOrderInvoicingStatus InvoiceStatus { get; private set; }
    public string? Notes { get; private set; }

    // Totals ? computed khi AddLine / RemoveLine
    public decimal SubTotal { get; private set; } // before VAT, before discount
    public decimal TotalDiscount { get; private set; }
    public decimal TotalVAT { get; private set; }
    public decimal TotalAmount { get; private set; } 

    public IReadOnlyCollection<SalesOrderLine> Lines => _lines.AsReadOnly();

    private SalesOrder()
    {
    }

    public static SalesOrder CreateDraft(string orderNumber, int customerId, string customerName, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new BusinessException("Order number is required.");

        if (customerId <= 0)
            throw new ValidationException("customerId", "CustomerId is required.");

        if (string.IsNullOrWhiteSpace(customerName))
            throw new ValidationException("customerName", "CustomerName is required.");

        return new SalesOrder
        {
            OrderNumber = orderNumber.Trim().ToUpperInvariant(),
            CustomerId = customerId,
            CustomerName = customerName.Trim(),
            OrderDate = DateTime.UtcNow,
            Notes = notes?.Trim(),
            Status = SalesOrderStatus.Draft,
            InvoiceStatus = SalesOrderInvoicingStatus.NotInvoiced,
        };
    }

    public void RecalculateTotals()
    {
        SubTotal = _lines.Sum(l => l.QuantityOrdered * l.UnitPrice);
        TotalDiscount = _lines.Sum(l => l.QuantityOrdered * l.UnitPrice * l.DiscountPercent / 100);
        TotalVAT = _lines.Sum(l => (l.QuantityOrdered * l.UnitPrice - (l.QuantityOrdered * l.UnitPrice * l.DiscountPercent / 100)) * l.VATRate / 100);
        TotalAmount = SubTotal - TotalDiscount + TotalVAT;
    }

    public void AddLine(int productId, string productName, decimal quantityOrdered, decimal unitPrice, decimal discountPercent, decimal vatRate)
    {
        if (Status != SalesOrderStatus.Draft)
            throw new BusinessException("Cannot modify a non-draft sales order.");

        bool existedProductId = _lines.Any(line => line.ProductId == productId);
        if (existedProductId)
            throw new BusinessException($"ProductId: {productId} has existed in this SO: {Id}");

        var line = SalesOrderLine.Create(productId, productName, quantityOrdered, unitPrice, discountPercent, vatRate);
        line.AssignToOrder(Id);
        _lines.Add(line);
        Touch();
    }

    public void UpdateDraft(int customerId, string customerName, string? notes)
    {
        if (Status != SalesOrderStatus.Draft)
            throw new BusinessException("Only draft sales order can be updated.");

        CustomerId = customerId;
        CustomerName = customerName;
        Notes = notes?.Trim();
        Touch();
    }

    public void ReplaceLines(
        IReadOnlyCollection<(int ProductId, string ProductName, decimal QuantityOrdered, decimal UnitPrice, decimal DiscountPercent, decimal VATRate)> lines)
    {
        if (Status != SalesOrderStatus.Draft)
            throw new BusinessException("Cannot modify a non-draft sales order.");

        _lines.Clear();

        foreach (var line in lines)
        {
            AddLine(line.ProductId, line.ProductName, line.QuantityOrdered, line.UnitPrice, line.DiscountPercent, line.VATRate);
        }
    }

    public void Approve()
    {
        if (Status != SalesOrderStatus.Draft)
            throw new BusinessException("Only draft sales orders can be confirmed.");

        if (_lines.Count == 0)
            throw new BusinessException("Cannot confirm a sales order without lines.");

        Status = SalesOrderStatus.Confirmed;
        ApprovedAt = DateTime.UtcNow;
        Touch();
    }

    public void Cancel()
    {
        if (Status is SalesOrderStatus.Delivered or SalesOrderStatus.PartialDelivery)
            throw new BusinessException("Sales orders has partial delivery or full delivery cannot be cancelled.");

        if (Status == SalesOrderStatus.Cancelled)
            throw new BusinessException("Sales order is already cancelled.");

        // Draft, Confirm can be Cancelled
        Status = SalesOrderStatus.Cancelled;
        Touch();
    }

    public void UpdateDeliveryStatus()
    {
        bool allDelivered = _lines.All(l => l.QuantityOrdered == l.QuantityDelivered);
        bool anyDelivered = _lines.Any(l => l.QuantityDelivered > 0);
        bool allNotDelivered = _lines.All(l => l.QuantityDelivered == 0);

        if (allDelivered)
            Status = SalesOrderStatus.Delivered;
        else if (anyDelivered)
            Status = SalesOrderStatus.PartialDelivery;
        else if (allNotDelivered)
            Status = SalesOrderStatus.Confirmed;

        Touch();
    }

    public void UpdateInvoiceStatus()
    {
        if (_lines.All(l => l.QuantityDelivered == 0))
            InvoiceStatus = SalesOrderInvoicingStatus.NotInvoiced;
        else if (_lines.All(l => l.QuantityDelivered == l.QuantityOrdered))
            InvoiceStatus = SalesOrderInvoicingStatus.FullyInvoiced;
        else
            InvoiceStatus = SalesOrderInvoicingStatus.PartiallyInvoiced;

        Touch();
    }
}
