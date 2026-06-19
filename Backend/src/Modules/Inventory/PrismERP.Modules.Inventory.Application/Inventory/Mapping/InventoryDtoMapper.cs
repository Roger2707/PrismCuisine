using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Application.Inventory.Mapping;

internal static class InventoryDtoMapper
{
    public static InventoryBalanceDto ToBalanceDto(
        InventoryBalance balance,
        decimal reservedQuantity) =>
        new(
            balance.Id,
            balance.ProductId,
            balance.WarehouseId,
            balance.QuantityOnHand,
            reservedQuantity,
            balance.GetAvailable(reservedQuantity),
            balance.ReorderLevel,
            balance.IsBelowReorderLevel());

    public static InventoryMovementDto ToMovementDto(InventoryMovement movement) =>
        new(
            movement.Id,
            movement.InventoryBalanceId,
            movement.MovementType.ToString(),
            movement.Quantity,
            movement.UnitCost,
            movement.ReferenceType.ToString(),
            movement.Reference,
            movement.ReferenceId,
            movement.Notes,
            movement.CreatedAt);

    public static InventoryCostLayerDto ToLayerDto(InventoryCostLayer layer) =>
        new(
            layer.Id,
            layer.InventoryBalanceId,
            layer.QuantityReceived,
            layer.QuantityRemaining,
            layer.UnitCost,
            layer.ReceivedAt);

    public static InventoryReservationDto ToReservationDto(InventoryReservation reservation) =>
        new(
            reservation.Id,
            reservation.InventoryBalanceId,
            reservation.Quantity,
            reservation.FulfilledQuantity,
            reservation.RemainingQuantity,
            reservation.Status.ToString(),
            reservation.ReferenceType.ToString(),
            reservation.ReferenceId,
            reservation.Notes);
}
