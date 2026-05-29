using PrismCuisine.BuildingBlocks.Domain.Entities;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.Inventory.Domain.Enums;

namespace PrismCuisine.Modules.Inventory.Domain.Entities;

public sealed class InventoryMovement : Entity
{
    public Guid InventoryBalanceId { get; private set; }
    public InventoryMovementType MovementType { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public InventoryReferenceType ReferenceType { get; private set; }
    public string? Reference { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string? Notes { get; private set; }

    private InventoryMovement()
    {
    }

    public static InventoryMovement Create(
        Guid inventoryBalanceId,
        InventoryMovementType movementType,
        decimal quantity,
        decimal unitCost,
        InventoryReferenceType referenceType,
        string? reference = null,
        Guid? referenceId = null,
        string? notes = null)
    {
        if (inventoryBalanceId == Guid.Empty)
        {
            throw new DomainException("InventoryBalanceId is required.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Movement quantity must be greater than zero.");
        }

        if (unitCost < 0)
        {
            throw new DomainException("Unit cost cannot be negative.");
        }

        return new InventoryMovement
        {
            InventoryBalanceId = inventoryBalanceId,
            MovementType = movementType,
            Quantity = quantity,
            UnitCost = unitCost,
            ReferenceType = referenceType,
            Reference = reference?.Trim(),
            ReferenceId = referenceId,
            Notes = notes?.Trim()
        };
    }
}
