using PrismERP.BuildingBlocks.Domain.Entities;
using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Inventory.Domain.Enums;

namespace PrismERP.Modules.Inventory.Domain.Entities;

public sealed class InventoryMovement : Entity
{
    public int InventoryBalanceId { get; private set; }
    public InventoryMovementType MovementType { get; private set; }
    public int InventoryCostLayerId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public InventoryReferenceType ReferenceType { get; private set; }
    public string? Reference { get; private set; }
    public int? ReferenceId { get; private set; }
    public string? Notes { get; private set; }

    private InventoryMovement()
    {
    }

    public static InventoryMovement Create(
        int inventoryBalanceId,
        InventoryMovementType movementType,
        decimal quantity,
        decimal unitCost,
        InventoryReferenceType referenceType,
        int inventoryCostLayerId = 0,
        string? reference = null,
        int? referenceId = null,
        string? notes = null)
    {
        if (inventoryBalanceId <= 0)
        {
            throw new BusinessException("InventoryBalanceId is required.");
        }

        if (quantity <= 0)
        {
            throw new BusinessException("Movement quantity must be greater than zero.");
        }

        if (unitCost < 0)
        {
            throw new BusinessException("Unit cost cannot be negative.");
        }

        return new InventoryMovement
        {
            InventoryBalanceId = inventoryBalanceId,
            MovementType = movementType,
            InventoryCostLayerId = inventoryCostLayerId,
            Quantity = quantity,
            UnitCost = unitCost,
            ReferenceType = referenceType,
            Reference = reference?.Trim(),
            ReferenceId = referenceId,
            Notes = notes?.Trim()
        };
    }
}
