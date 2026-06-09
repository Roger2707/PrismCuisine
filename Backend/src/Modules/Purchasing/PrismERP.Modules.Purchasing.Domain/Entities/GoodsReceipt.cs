using PrismERP.BuildingBlocks.Domain.Aggregates;
using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Purchasing.Domain.Enums;

namespace PrismERP.Modules.Purchasing.Domain.Entities;

public sealed class GoodsReceipt : AggregateRoot
{
    private readonly List<GoodsReceiptLine> _lines = [];

    public string ReceiptNumber { get; private set; } = null!;
    public int PurchaseOrderId { get; private set; }
    public GoodsReceiptStatus Status { get; private set; }
    public DateTime? PostedAt { get; private set; }
    public string? Notes { get; private set; }
    public IReadOnlyCollection<GoodsReceiptLine> Lines => _lines.AsReadOnly();

    private GoodsReceipt()
    {
    }

    public static GoodsReceipt CreateDraft(int purchaseOrderId, string receiptNumber, string? notes = null)
    {
        if (purchaseOrderId <= 0)
        {
            throw new DomainException("PurchaseOrderId is required.");
        }

        if (string.IsNullOrWhiteSpace(receiptNumber))
        {
            throw new DomainException("Receipt number is required.");
        }

        return new GoodsReceipt
        {
            PurchaseOrderId = purchaseOrderId,
            ReceiptNumber = receiptNumber.Trim().ToUpperInvariant(),
            Status = GoodsReceiptStatus.Draft,
            Notes = notes?.Trim()
        };
    }

    public void UpdateDraft(string? notes)
    {
        if (Status != GoodsReceiptStatus.Draft)
        {
            throw new DomainException("Only draft goods receipts can be updated.");
        }

        Notes = notes?.Trim();
        Touch();
    }

    public void AddLine(int purchaseOrderLineId, int productId, decimal quantity, decimal unitCost)
    {
        if (Status != GoodsReceiptStatus.Draft)
        {
            throw new DomainException("Cannot modify a non-draft goods receipt.");
        }

        if (_lines.Any(l => l.PurchaseOrderLineId == purchaseOrderLineId))
        {
            throw new DomainException("Purchase order line already exists on this goods receipt.");
        }

        var line = GoodsReceiptLine.Create(purchaseOrderLineId, productId, quantity, unitCost);
        line.AssignToReceipt(Id);
        _lines.Add(line);
        Touch();
    }

    public void ReplaceLines(
        IReadOnlyCollection<(int PurchaseOrderLineId, int ProductId, decimal Quantity, decimal UnitCost)> lines)
    {
        if (Status != GoodsReceiptStatus.Draft)
        {
            throw new DomainException("Cannot modify a non-draft goods receipt.");
        }

        _lines.Clear();

        foreach (var line in lines)
        {
            AddLine(line.PurchaseOrderLineId, line.ProductId, line.Quantity, line.UnitCost);
        }
    }

    public void Post()
    {
        if (Status != GoodsReceiptStatus.Draft)
        {
            throw new DomainException("Only draft goods receipts can be posted.");
        }

        if (_lines.Count == 0)
        {
            throw new DomainException("Cannot post a goods receipt without lines.");
        }

        Status = GoodsReceiptStatus.Posted;
        PostedAt = DateTime.UtcNow;
        Touch();
    }
}
