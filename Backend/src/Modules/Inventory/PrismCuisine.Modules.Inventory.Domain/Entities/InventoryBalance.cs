using PrismCuisine.BuildingBlocks.Domain.Aggregates;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;

namespace PrismCuisine.Modules.Inventory.Domain.Entities;

public sealed class InventoryBalance : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal QuantityOnHand { get; private set; }
    public decimal ReorderLevel { get; private set; }

    private InventoryBalance()
    {
    }

    public static InventoryBalance Create(Guid productId, Guid warehouseId, decimal reorderLevel)
    {
        if (productId == Guid.Empty)
        {
            throw new DomainException("ProductId is required.");
        }

        if (warehouseId == Guid.Empty)
        {
            throw new DomainException("WarehouseId is required.");
        }

        if (reorderLevel < 0)
        {
            throw new DomainException("Reorder level cannot be negative.");
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
            throw new DomainException("Quantity must be greater than zero.");
        }

        QuantityOnHand += quantity;
        Touch();
    }

    public void Decrease(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        if (QuantityOnHand - quantity < 0)
        {
            throw new DomainException("Insufficient on-hand quantity.");
        }

        QuantityOnHand -= quantity;
        Touch();
    }

    public void SetReorderLevel(decimal reorderLevel)
    {
        if (reorderLevel < 0)
        {
            throw new DomainException("Reorder level cannot be negative.");
        }

        ReorderLevel = reorderLevel;
        Touch();
    }

    public bool IsBelowReorderLevel() => QuantityOnHand <= ReorderLevel;

    public decimal GetAvailable(decimal reservedQuantity) => QuantityOnHand - reservedQuantity;
}
