using PrismCuisine.BuildingBlocks.Domain.Entities;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.Inventory.Domain.Enums;

namespace PrismCuisine.Modules.Inventory.Domain.Entities;

public sealed class InventoryReservation : Entity
{
    public Guid InventoryBalanceId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal FulfilledQuantity { get; private set; }
    public InventoryReservationStatus Status { get; private set; }
    public InventoryReferenceType ReferenceType { get; private set; }
    public Guid ReferenceId { get; private set; }
    public string? Notes { get; private set; }

    private InventoryReservation()
    {
    }

    public decimal RemainingQuantity => Quantity - FulfilledQuantity;

    public static InventoryReservation Create(
        Guid inventoryBalanceId,
        decimal quantity,
        InventoryReferenceType referenceType,
        Guid referenceId,
        string? notes = null)
    {
        if (inventoryBalanceId == Guid.Empty)
        {
            throw new DomainException("InventoryBalanceId is required.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Reservation quantity must be greater than zero.");
        }

        if (referenceId == Guid.Empty)
        {
            throw new DomainException("ReferenceId is required.");
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
            throw new DomainException("Only active reservations can be released.");
        }

        Status = InventoryReservationStatus.Released;
    }

    public decimal RecordFulfillment(decimal quantity)
    {
        if (Status != InventoryReservationStatus.Active)
        {
            throw new DomainException("Only active reservations can be fulfilled.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Fulfillment quantity must be greater than zero.");
        }

        if (quantity > RemainingQuantity)
        {
            throw new DomainException("Fulfillment quantity exceeds remaining reservation.");
        }

        FulfilledQuantity += quantity;

        if (FulfilledQuantity >= Quantity)
        {
            Status = InventoryReservationStatus.Fulfilled;
        }

        return quantity;
    }
}
