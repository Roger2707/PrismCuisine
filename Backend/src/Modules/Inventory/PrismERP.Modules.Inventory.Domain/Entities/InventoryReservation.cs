using PrismERP.BuildingBlocks.Domain.Entities;
using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Inventory.Domain.Enums;

namespace PrismERP.Modules.Inventory.Domain.Entities;

public sealed class InventoryReservation : Entity
{
    public int InventoryBalanceId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal FulfilledQuantity { get; private set; }
    public InventoryReservationStatus Status { get; private set; }
    public InventoryReferenceType ReferenceType { get; private set; }
    public int ReferenceId { get; private set; }
    public string? Notes { get; private set; }

    private InventoryReservation()
    {
    }

    public decimal RemainingQuantity => Quantity - FulfilledQuantity;

    public static InventoryReservation Create(
        int inventoryBalanceId,
        decimal quantity,
        InventoryReferenceType referenceType,
        int referenceId,
        string? notes = null)
    {
        if (inventoryBalanceId <= 0)
        {
            throw new BusinessException("InventoryBalanceId is required.");
        }

        if (quantity <= 0)
        {
            throw new BusinessException("Reservation quantity must be greater than zero.");
        }

        if (referenceId <= 0)
        {
            throw new BusinessException("ReferenceId is required.");
        }

        return new InventoryReservation
        {
            InventoryBalanceId = inventoryBalanceId,
            Quantity = quantity,
            FulfilledQuantity = 0m,
            Status = InventoryReservationStatus.Active,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Notes = notes?.Trim()
        };
    }

    public void Release()
    {
        if (Status != InventoryReservationStatus.Active)
        {
            throw new BusinessException("Only active reservations can be released.");
        }

        Status = InventoryReservationStatus.Released;
    }

    public decimal RecordFulfillment(decimal quantity)
    {
        if (Status != InventoryReservationStatus.Active)
        {
            throw new BusinessException("Only active reservations can be fulfilled.");
        }

        if (quantity <= 0)
        {
            throw new BusinessException("Fulfillment quantity must be greater than zero.");
        }

        if (quantity > RemainingQuantity)
        {
            throw new BusinessException("Fulfillment quantity exceeds remaining reservation.");
        }

        FulfilledQuantity += quantity;

        if (FulfilledQuantity >= Quantity)
        {
            Status = InventoryReservationStatus.Fulfilled;
        }

        return quantity;
    }

    public void ReverseFulfillment(decimal quantity)
    {
        if (Status is InventoryReservationStatus.Released)
        {
            throw new BusinessException("Released reservations cannot be reversed.");
        }

        if (quantity <= 0)
        {
            throw new BusinessException("Reversal quantity must be greater than zero.");
        }

        if (quantity > FulfilledQuantity)
        {
            throw new BusinessException("Reversal quantity exceeds fulfilled reservation quantity.");
        }

        FulfilledQuantity -= quantity;

        if (FulfilledQuantity < Quantity)
        {
            Status = InventoryReservationStatus.Active;
        }
    }
}
