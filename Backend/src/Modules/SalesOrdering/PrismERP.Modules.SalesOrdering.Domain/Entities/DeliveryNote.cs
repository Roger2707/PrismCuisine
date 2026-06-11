using PrismERP.BuildingBlocks.Domain.Aggregates;
using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.SalesOrdering.Domain.Enums;

namespace PrismERP.Modules.SalesOrdering.Domain.Entities;

public sealed class DeliveryNote : AggregateRoot
{
    private readonly List<DeliveryNoteLine> _lines = [];

    private DeliveryNote() { } // EF Core

    public string DeliveryNumber { get; private set; } = null!;  // DN2024001
    public int SalesOrderId { get; private set; }

    // Snapshot t? SalesOrder t?i th?i ?i?m t?o
    public int CustomerId { get; private set; }
    public string CustomerName { get; private set; } = null!;
    public string OrderNumber { get; private set; } = null!;

    public DateTime DeliveryDate { get; private set; }
    public DeliveryNoteStatus Status { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<DeliveryNoteLine> Lines => _lines.AsReadOnly();

    // --- Factory ---
    public static DeliveryNote CreateDraft(
        string deliveryNumber,
        int salesOrderId,
        int customerId,
        string customerName,
        string orderNumber,
        SalesOrderStatus salesOrderStatus,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(deliveryNumber))
            throw new BusinessException("DeliveryNumber is required.");

        if (salesOrderStatus != SalesOrderStatus.Confirmed && salesOrderStatus != SalesOrderStatus.PartialDelivery)
            throw new BusinessException("SalesOrder must be Confirmed or PartialDelivery.");

        return new DeliveryNote
        {
            DeliveryNumber = deliveryNumber,
            SalesOrderId = salesOrderId,
            CustomerId = customerId,     // snapshot
            CustomerName = customerName,   // snapshot
            OrderNumber = orderNumber,    // snapshot
            DeliveryDate = DateTime.UtcNow,
            Status = DeliveryNoteStatus.Draft,
            Notes = notes
        };
    }

    public void UpdateDraft(string? notes)
    {
        if (Status != DeliveryNoteStatus.Draft)
            throw new BusinessException("Only draft Delivery Note can be updated.");

        Notes = notes?.Trim();
        Touch();
    }

    public void ReplaceLines(
        IReadOnlyCollection<(decimal DeliveredQuantity, SalesOrderLine SalesOrderLine)> lines)
    {
        if (Status != DeliveryNoteStatus.Draft)
            throw new BusinessException("Cannot modify a non-draft Delivery.");

        if (lines.Count == 0)
            throw new BusinessException("Delivery note must have at least one line.");

        _lines.Clear();

        foreach (var line in lines)
        {
            AddLine(line.DeliveredQuantity, line.SalesOrderLine);
        }

        Touch();
    }

    // --- Business methods ---
    public void AddLine(decimal deliveredQuantity, SalesOrderLine salesOrderLine)
    {
        if (Status != DeliveryNoteStatus.Draft)
            throw new BusinessException("Only Draft delivery notes can be modified.");
        if (deliveredQuantity <= 0)
            throw new BusinessException("Quantity must be greater than zero.");

        var existing = _lines.FirstOrDefault(l => l.SalesOrderLineId == salesOrderLine.Id);
        if (existing is not null)
            throw new BusinessException($"{salesOrderLine.ProductName}: line already added.");

        if (deliveredQuantity > salesOrderLine.QuantityRemaining)
            throw new BusinessException(
                $"{salesOrderLine.ProductName}: delivery quantity exceeds remaining ({salesOrderLine.QuantityRemaining}).");

        _lines.Add(DeliveryNoteLine.Create(
            salesOrderLine.Id,
            salesOrderLine.ProductId,
            salesOrderLine.ProductName,
            deliveredQuantity));
    }

    public void Post(SalesOrder salesOrder)
    {
        if (Status != DeliveryNoteStatus.Draft)
            throw new BusinessException("Only Draft delivery notes can be posted.");
        if (!_lines.Any())
            throw new BusinessException("Delivery note must have at least one line.");
        if(salesOrder.Status != SalesOrderStatus.Confirmed && salesOrder.Status != SalesOrderStatus.PartialDelivery)
            throw new BusinessException("Sales order must be Confirmed or PartialDelivery.");

        // Update QuantityDelivered tren t?ng OrderLine
        foreach (var line in _lines)
        {
            var orderLine = salesOrder.Lines
                .FirstOrDefault(l => l.Id == line.SalesOrderLineId)
                ?? throw new NotFoundException($"SalesOrderLine {line.SalesOrderLineId} not found.");

            orderLine.RecordDelivery(line.QuantityDelivered);
        }

        // Update status SalesOrder
        salesOrder.UpdateDeliveryStatus();

        Status = DeliveryNoteStatus.Posted;
    }

    public void Cancel(SalesOrder salesOrder)
    {
        if (Status != DeliveryNoteStatus.Posted)
            throw new BusinessException("Only Posted delivery notes can be cancelled.");

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
