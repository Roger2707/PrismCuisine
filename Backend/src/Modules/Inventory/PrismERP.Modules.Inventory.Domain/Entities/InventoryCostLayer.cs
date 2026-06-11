using PrismERP.BuildingBlocks.Domain.Entities;
using PrismERP.BuildingBlocks.Domain.Exceptions;

namespace PrismERP.Modules.Inventory.Domain.Entities;

public sealed class InventoryCostLayer : Entity
{
    public int InventoryBalanceId { get; private set; }
    public decimal QuantityReceived { get; private set; }
    public decimal QuantityRemaining { get; private set; }
    public decimal UnitCost { get; private set; }
    public DateTime ReceivedAt { get; private set; }

    private InventoryCostLayer()
    {
    }

    public static InventoryCostLayer Create(
        int inventoryBalanceId,
        decimal quantity,
        decimal unitCost,
        DateTime? receivedAt = null)
    {
        if (inventoryBalanceId <= 0)
        {
            throw new BusinessException("InventoryBalanceId is required.");
        }

        if (quantity <= 0)
        {
            throw new BusinessException("Layer quantity must be greater than zero.");
        }

        if (unitCost < 0)
        {
            throw new BusinessException("Unit cost cannot be negative.");
        }

        return new InventoryCostLayer
        {
            InventoryBalanceId = inventoryBalanceId,
            QuantityReceived = quantity,
            QuantityRemaining = quantity,
            UnitCost = unitCost,
            ReceivedAt = receivedAt ?? DateTime.UtcNow
        };
    }

    public decimal Consume(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new BusinessException("Consume quantity must be greater than zero.");
        }

        if (quantity > QuantityRemaining)
        {
            throw new BusinessException("Cannot consume more than layer remaining quantity.");
        }

        QuantityRemaining -= quantity;
        return UnitCost;
    }

    public void Restore(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new BusinessException("Restore quantity must be greater than zero.");
        }

        if (QuantityRemaining + quantity > QuantityReceived)
        {
            throw new BusinessException("Cannot restore more than was consumed from this cost layer.");
        }

        QuantityRemaining += quantity;
    }
}
