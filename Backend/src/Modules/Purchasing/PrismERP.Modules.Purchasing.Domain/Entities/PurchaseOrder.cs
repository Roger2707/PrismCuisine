using PrismERP.BuildingBlocks.Domain.Aggregates;
using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Purchasing.Domain.Enums;
using PrismERP.Modules.Purchasing.Domain.Events;

namespace PrismERP.Modules.Purchasing.Domain.Entities;

public sealed class PurchaseOrder : AggregateRoot
{
    private readonly List<PurchaseOrderLine> _lines = [];

    public string OrderNumber { get; private set; } = null!;
    public int SupplierId { get; private set; }
    public int WarehouseId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public PurchaseOrderInvoicingStatus InvoiceStatus { get; private set; }
    public int? AmendedFromPurchaseOrderId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? Notes { get; private set; }
    public IReadOnlyCollection<PurchaseOrderLine> Lines => _lines.AsReadOnly();

    private PurchaseOrder()
    {
    }

    public static PurchaseOrder CreateDraft(
        string orderNumber,
        int supplierId,
        int warehouseId,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new BusinessException("Order number is required.");
        }

        if (supplierId <= 0)
        {
            throw new ValidationException("supplierId", "SupplierId is required.");
        }

        if (warehouseId <= 0)
        {
            throw new ValidationException("warehouseId", "WarehouseId is required.");
        }

        return new PurchaseOrder
        {
            OrderNumber = orderNumber.Trim().ToUpperInvariant(),
            SupplierId = supplierId,
            WarehouseId = warehouseId,
            Status = PurchaseOrderStatus.Draft,
            Notes = notes?.Trim()
        };
    }

    public static PurchaseOrder CreateAmendment(
        string orderNumber,
        PurchaseOrder source,
        string? notes)
    {
        if (source.Status is not PurchaseOrderStatus.Approved and not PurchaseOrderStatus.PartiallyReceived)
        {
            throw new BusinessException("Only approved or partially received purchase orders can be amended.");
        }

        var amendment = CreateDraft(orderNumber, source.SupplierId, source.WarehouseId, notes);
        amendment.AmendedFromPurchaseOrderId = source.Id;
        return amendment;
    }

    public void UpdateDraft(int supplierId, int warehouseId, string? notes)
    {
        if (Status != PurchaseOrderStatus.Draft)
        {
            throw new BusinessException("Only draft purchase orders can be updated.");
        }

        if (supplierId <= 0)
        {
            throw new ValidationException("supplierId", "SupplierId is required.");
        }

        if (warehouseId <= 0)
        {
            throw new ValidationException("warehouseId", "WarehouseId is required.");
        }

        SupplierId = supplierId;
        WarehouseId = warehouseId;
        Notes = notes?.Trim();
        Touch();
    }

    public void AddLine(int productId, decimal quantityOrdered, decimal unitPrice)
    {
        if (Status != PurchaseOrderStatus.Draft)
        {
            throw new BusinessException("Cannot modify a non-draft purchase order.");
        }

        if (_lines.Any(l => l.ProductId == productId))
        {
            throw new BusinessException("Product already exists on this purchase order.");
        }

        var line = PurchaseOrderLine.Create(productId, quantityOrdered, unitPrice);
        line.AssignToOrder(Id);
        _lines.Add(line);
        Touch();
    }

    public void ReplaceLines(IReadOnlyCollection<(int ProductId, decimal QuantityOrdered, decimal UnitPrice)> lines)
    {
        if (Status != PurchaseOrderStatus.Draft)
        {
            throw new BusinessException("Cannot modify lines of a non-draft purchase order.");
        }

        _lines.Clear();

        foreach (var line in lines)
        {
            AddLine(line.ProductId, line.QuantityOrdered, line.UnitPrice);
        }
    }

    public void Approve()
    {
        if (Status != PurchaseOrderStatus.Draft)
        {
            throw new BusinessException("Only draft purchase orders can be approved.");
        }

        if (_lines.Count == 0)
        {
            throw new BusinessException("Cannot approve a purchase order without lines.");
        }

        Status = PurchaseOrderStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        Touch();

        RaiseDomainEvent(new PurchaseOrderApprovedEvent(Id, OrderNumber));
    }

    public void Cancel()
    {
        if (Status is PurchaseOrderStatus.Received or PurchaseOrderStatus.PartiallyReceived)
        {
            throw new BusinessException("Purchase order with receipts cannot be cancelled.");
        }

        if (Status == PurchaseOrderStatus.Cancelled)
        {
            throw new BusinessException("Purchase order is already cancelled.");
        }

        Status = PurchaseOrderStatus.Cancelled;
        Touch();
    }

    public void RecordReceipt(int purchaseOrderLineId, decimal quantityReceived)
    {
        if (Status is not PurchaseOrderStatus.Approved and not PurchaseOrderStatus.PartiallyReceived)
        {
            throw new BusinessException("Cannot receive goods for a purchase order that is not approved.");
        }

        var line = _lines.FirstOrDefault(l => l.Id == purchaseOrderLineId)
            ?? throw new NotFoundException($"Purchase order line '{purchaseOrderLineId}' was not found.");

        line.RecordReceipt(quantityReceived);
        RefreshReceivingStatus();
        Touch();
    }

    private void RefreshReceivingStatus()
    {
        if (_lines.All(l => l.IsFullyReceived()))
        {
            Status = PurchaseOrderStatus.Received;
            return;
        }

        if (_lines.Any(l => l.QuantityReceived > 0))
        {
            Status = PurchaseOrderStatus.PartiallyReceived;
        }
    }

    public void UpdateInvoiceStatus(PurchaseOrderInvoicingStatus status)
    {
        InvoiceStatus = status;
        Touch();
    }
}
