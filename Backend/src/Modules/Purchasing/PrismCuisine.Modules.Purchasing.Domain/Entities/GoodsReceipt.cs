using PrismCuisine.BuildingBlocks.Domain.Aggregates;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.Purchasing.Domain.Enums;

namespace PrismCuisine.Modules.Purchasing.Domain.Entities;

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
