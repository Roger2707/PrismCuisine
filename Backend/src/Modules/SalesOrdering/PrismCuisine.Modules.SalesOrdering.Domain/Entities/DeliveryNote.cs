using PrismCuisine.BuildingBlocks.Domain.Aggregates;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.SalesOrdering.Domain.Enums;

namespace PrismCuisine.Modules.SalesOrdering.Domain.Entities;

public sealed class DeliveryNote : AggregateRoot
{
    private readonly List<DeliveryNoteLine> _lines = [];

    private DeliveryNote() { } // EF Core

    public string DeliveryNumber { get; private set; } = null!;  // DN2024001
    public int SalesOrderId { get; private set; }

    // Snapshot từ SalesOrder tại thời điểm tạo
    public int CustomerId { get; private set; }
    public string CustomerName { get; private set; } = null!;
    public string OrderNumber { get; private set; } = null!;

    public DateTime DeliveryDate { get; private set; }
    public DeliveryNoteStatus Status { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<DeliveryNoteLine> Lines => _lines.AsReadOnly();

    // --- Factory ---
    public static DeliveryNote Create(
        string deliveryNumber,
        int salesOrderId,
        int customerId,
        string customerName,
        string orderNumber,
        SalesOrderStatus salesOrderStatus,
        DateTime deliveryDate,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(deliveryNumber))
            throw new DomainException("DeliveryNumber is required.");
        if (salesOrderStatus != SalesOrderStatus.Confirmed && salesOrderStatus != SalesOrderStatus.PartialDelivery)
            throw new DomainException("SalesOrder must be Confirmed or PartialDelivery.");

        return new DeliveryNote
        {
            DeliveryNumber = deliveryNumber,
            SalesOrderId = salesOrderId,
            CustomerId = customerId,     // snapshot
            CustomerName = customerName,   // snapshot
            OrderNumber = orderNumber,    // snapshot
            DeliveryDate = deliveryDate,
            Status = DeliveryNoteStatus.Draft,
            Notes = notes
        };
    }

    public void UpdateDraft(string? notes)
    {
        if (Status != DeliveryNoteStatus.Draft)
            throw new DomainException("Only draft Delivery Note can be updated.");

        Notes = notes?.Trim();
        Touch();
    }

    public void ReplaceLines(
        IReadOnlyCollection<(int salesOrderLineId, int productId, string productName, decimal deliveredQuantity, decimal remainingQuantity)> lines)
    {
        if (Status != DeliveryNoteStatus.Draft)
            throw new DomainException("Cannot modify a non-draft sales order.");

        _lines.Clear();

        foreach (var line in lines)
        {
            AddLine(line.salesOrderLineId, line.productId, line.productName, line.deliveredQuantity, line.remainingQuantity);
        }
    }

    // --- Business methods ---
    public void AddLine(int salesOrderLineId, int productId, string productName, decimal deliveredQuantity, decimal remainingQuantity)
    {
        if (Status != DeliveryNoteStatus.Draft)
            throw new DomainException("Only Draft delivery notes can be modified.");
        if (deliveredQuantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        if (deliveredQuantity > remainingQuantity)
            throw new DomainException(
                $"{productName}: quantity exceeds remaining ({remainingQuantity}).");

        var existing = _lines.FirstOrDefault(l => l.SalesOrderLineId == salesOrderLineId);
        if (existing is not null)
            throw new DomainException($"{productName}: line already added.");

        _lines.Add(DeliveryNoteLine.Create(salesOrderLineId, productId, productName, deliveredQuantity));
    }

    public void Post(SalesOrder salesOrder)
    {
        if (Status != DeliveryNoteStatus.Draft)
            throw new DomainException("Only Draft delivery notes can be posted.");
        if (!_lines.Any())
            throw new DomainException("Delivery note must have at least one line.");

        // Update QuantityDelivered trên từng OrderLine
        foreach (var line in _lines)
        {
            var orderLine = salesOrder.Lines
                .FirstOrDefault(l => l.Id == line.SalesOrderLineId)
                ?? throw new DomainException($"SalesOrderLine {line.SalesOrderLineId} not found.");

            orderLine.RecordDelivery(line.QuantityDelivered);
        }

        // Update status SalesOrder
        salesOrder.UpdateDeliveryStatus();

        Status = DeliveryNoteStatus.Posted;
    }

    public void Cancel(SalesOrder salesOrder)
    {
        if (Status != DeliveryNoteStatus.Posted)
            throw new DomainException("Only Posted delivery notes can be cancelled.");

        // Rollback QuantityDelivered
        foreach (var line in _lines)
        {
            var orderLine = salesOrder.Lines
                .First(l => l.Id == line.SalesOrderLineId);

            orderLine.RollbackDelivery(line.QuantityDelivered);
        }

        salesOrder.UpdateDeliveryStatus();
        Status = DeliveryNoteStatus.Cancelled;
    }
}
