using PrismCuisine.BuildingBlocks.Domain.Aggregates;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.Purchasing.Domain.Enums;
using PrismCuisine.Modules.Purchasing.Domain.Events;

namespace PrismCuisine.Modules.Purchasing.Domain.Entities;

public sealed class PurchaseOrder : AggregateRoot
{
    private readonly List<PurchaseOrderLine> _lines = [];

    public string OrderNumber { get; private set; } = null!;
    public int SupplierId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public DateTime? PostedAt { get; private set; }
    public IReadOnlyCollection<PurchaseOrderLine> Lines => _lines.AsReadOnly();

    private PurchaseOrder()
    {
    }

    public static PurchaseOrder CreateDraft(string orderNumber, int supplierId)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new DomainException("Order number is required.");
        }

        if (supplierId <= 0)
        {
            throw new DomainException("SupplierId is required.");
        }

        return new PurchaseOrder
        {
            OrderNumber = orderNumber.Trim().ToUpperInvariant(),
            SupplierId = supplierId,
            Status = PurchaseOrderStatus.Draft
        };
    }

    public void AddLine(int productId, decimal quantity, decimal unitPrice)
    {
        if (Status != PurchaseOrderStatus.Draft)
        {
            throw new DomainException("Cannot modify a non-draft purchase order.");
        }

        var line = PurchaseOrderLine.Create(productId, quantity, unitPrice);
        line.AssignToOrder(Id);
        _lines.Add(line);
        Touch();
    }

    public void Post()
    {
        if (Status == PurchaseOrderStatus.Posted)
        {
            throw new DomainException("Purchase order is already posted.");
        }

        if (Status == PurchaseOrderStatus.Cancelled)
        {
            throw new DomainException("Cancelled purchase order cannot be posted.");
        }

        if (_lines.Count == 0)
        {
            throw new DomainException("Cannot post a purchase order without lines.");
        }

        Status = PurchaseOrderStatus.Posted;
        PostedAt = DateTime.UtcNow;
        Touch();

        RaiseDomainEvent(new PurchaseOrderPostedEvent(Id, OrderNumber));
    }

    public void Cancel()
    {
        if (Status == PurchaseOrderStatus.Posted)
        {
            throw new DomainException("Posted purchase order cannot be cancelled.");
        }

        Status = PurchaseOrderStatus.Cancelled;
        Touch();
    }
}
