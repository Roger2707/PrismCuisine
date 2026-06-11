using PrismERP.BuildingBlocks.Domain.Entities;
using PrismERP.BuildingBlocks.Domain.Exceptions;

namespace PrismERP.Modules.Purchasing.Domain.Entities;

public sealed class GoodsReceiptLine : Entity
{
    public int GoodsReceiptId { get; private set; }
    public int PurchaseOrderLineId { get; private set; }
    public int ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }

    private GoodsReceiptLine()
    {
    }

    internal static GoodsReceiptLine Create(
        int purchaseOrderLineId,
        int productId,
        decimal quantity,
        decimal unitCost)
    {
        if (purchaseOrderLineId <= 0)
        {
            throw new BusinessException("PurchaseOrderLineId is required.");
        }

        if (productId <= 0)
        {
            throw new BusinessException("ProductId is required.");
        }

        if (quantity <= 0)
        {
            throw new BusinessException("Quantity must be greater than zero.");
        }

        if (unitCost < 0)
        {
            throw new BusinessException("Unit cost cannot be negative.");
        }

        return new GoodsReceiptLine
        {
            PurchaseOrderLineId = purchaseOrderLineId,
            ProductId = productId,
            Quantity = quantity,
            UnitCost = unitCost
        };
    }

    internal void AssignToReceipt(int goodsReceiptId) => GoodsReceiptId = goodsReceiptId;
}
