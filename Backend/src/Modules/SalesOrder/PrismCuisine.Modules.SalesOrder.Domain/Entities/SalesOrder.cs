using PrismCuisine.BuildingBlocks.Domain.Aggregates;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.SalesOrder.Domain.Enums;

namespace PrismCuisine.Modules.SalesOrder.Domain.Entities;

public sealed class SalesOrder : AggregateRoot
{
    private readonly List<SalesOrderLine> _lines = [];

    public string OrderNumber { get; private set; } = null!;
    public Guid CustomerId { get; private set; }
    public SalesOrderStatus Status { get; private set; }
    public IReadOnlyCollection<SalesOrderLine> Lines => _lines.AsReadOnly();

    private SalesOrder()
    {
    }

    public static SalesOrder CreateDraft(string orderNumber, Guid customerId)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new DomainException("Order number is required.");
        }

        if (customerId == Guid.Empty)
        {
            throw new DomainException("CustomerId is required.");
        }

        return new SalesOrder
        {
            OrderNumber = orderNumber.Trim().ToUpperInvariant(),
            CustomerId = customerId,
            Status = SalesOrderStatus.Draft
        };
    }

    public void AddLine(Guid productId, decimal quantity, decimal unitPrice)
    {
        if (Status != SalesOrderStatus.Draft)
        {
            throw new DomainException("Cannot modify a non-draft sales order.");
        }

        var line = SalesOrderLine.Create(productId, quantity, unitPrice);
        line.AssignToOrder(Id);
        _lines.Add(line);
        Touch();
    }

    public void Confirm()
    {
        if (Status != SalesOrderStatus.Draft)
        {
            throw new DomainException("Only draft sales orders can be confirmed.");
        }

        if (_lines.Count == 0)
        {
            throw new DomainException("Cannot confirm a sales order without lines.");
        }

        Status = SalesOrderStatus.Confirmed;
        Touch();
    }
}
