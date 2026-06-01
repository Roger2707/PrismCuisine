using PrismCuisine.BuildingBlocks.Domain.Entities;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;

namespace PrismCuisine.Modules.Inventory.Domain.Entities;

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
            throw new DomainException("InventoryBalanceId is required.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Layer quantity must be greater than zero.");
        }

        if (unitCost < 0)
        {
            throw new DomainException("Unit cost cannot be negative.");
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
            throw new DomainException("Consume quantity must be greater than zero.");
        }

        if (quantity > QuantityRemaining)
        {
            throw new DomainException("Cannot consume more than layer remaining quantity.");
        }

        QuantityRemaining -= quantity;
        return UnitCost;
    }
}
