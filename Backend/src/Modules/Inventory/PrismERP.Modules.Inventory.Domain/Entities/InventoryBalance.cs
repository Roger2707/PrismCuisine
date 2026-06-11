using PrismERP.BuildingBlocks.Domain.Aggregates;
using PrismERP.BuildingBlocks.Domain.Exceptions;

namespace PrismERP.Modules.Inventory.Domain.Entities;

public sealed class InventoryBalance : AggregateRoot
{
    public int ProductId { get; private set; }
    public int WarehouseId { get; private set; }
    public decimal QuantityOnHand { get; private set; }
    public decimal ReorderLevel { get; private set; }

    private InventoryBalance()
    {
    }

    public static InventoryBalance Create(int productId, int warehouseId, decimal reorderLevel)
    {
        if (productId <= 0)
        {
            throw new BusinessException("ProductId is required.");
        }

        if (warehouseId <= 0)
        {
            throw new BusinessException("WarehouseId is required.");
        }

        if (reorderLevel < 0)
        {
            throw new BusinessException("Reorder level cannot be negative.");
        }

        return new InventoryBalance
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            QuantityOnHand = 0m,
            ReorderLevel = reorderLevel
        };
    }

    public void Increase(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new BusinessException("Quantity must be greater than zero.");
        }

        QuantityOnHand += quantity;
        Touch();
    }

    public void Decrease(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new BusinessException("Quantity must be greater than zero.");
        }

        if (QuantityOnHand - quantity < 0)
        {
            throw new BusinessException("Insufficient on-hand quantity.");
        }

        QuantityOnHand -= quantity;
        Touch();
    }

    public void SetReorderLevel(decimal reorderLevel)
    {
        if (reorderLevel < 0)
        {
            throw new BusinessException("Reorder level cannot be negative.");
        }

        ReorderLevel = reorderLevel;
        Touch();
    }

    public bool IsBelowReorderLevel() => QuantityOnHand <= ReorderLevel;

    public decimal GetAvailable(decimal reservedQuantity) => QuantityOnHand - reservedQuantity;
}
