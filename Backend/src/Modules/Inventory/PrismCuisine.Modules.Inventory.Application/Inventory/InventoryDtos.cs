namespace PrismCuisine.Modules.Inventory.Application.Inventory;

public sealed record InventoryBalanceDto(
    Guid Id,
    Guid ProductId,
    Guid WarehouseId,
    decimal QuantityOnHand,
    decimal ReservedQuantity,
    decimal AvailableQuantity,
    decimal ReorderLevel,
    bool IsBelowReorderLevel);

public sealed record InventoryMovementDto(
    Guid Id,
    Guid InventoryBalanceId,
    string MovementType,
    decimal Quantity,
    decimal UnitCost,
    string ReferenceType,
    string? Reference,
    Guid? ReferenceId,
    string? Notes,
    DateTime CreatedAt);

public sealed record InventoryCostLayerDto(
    Guid Id,
    Guid InventoryBalanceId,
    decimal QuantityReceived,
    decimal QuantityRemaining,
    decimal UnitCost,
    DateTime ReceivedAt);

public sealed record InventoryReservationDto(
    Guid Id,
    Guid InventoryBalanceId,
    decimal Quantity,
    decimal FulfilledQuantity,
    decimal RemainingQuantity,
    string Status,
    string ReferenceType,
    Guid ReferenceId,
    string? Notes);

public sealed record CreateInventoryBalanceRequest(
    Guid ProductId,
    Guid WarehouseId,
    decimal ReorderLevel);

public sealed record ReceiveInventoryRequest(
    Guid ProductId,
    Guid WarehouseId,
    decimal Quantity,
    decimal UnitCost,
    string? Reference = null,
    Guid? ReferenceId = null,
    string? Notes = null);

public sealed record IssueInventoryRequest(
    Guid ProductId,
    Guid WarehouseId,
    decimal Quantity,
    string? Reference = null,
    Guid? ReferenceId = null,
    string? Notes = null);

public sealed record AdjustInventoryRequest(
    Guid ProductId,
    Guid WarehouseId,
    decimal NewQuantity,
    decimal UnitCostForIncrease,
    string? Reference = null,
    string? Notes = null);

public sealed record CreateReservationRequest(
    Guid ProductId,
    Guid WarehouseId,
    decimal Quantity,
    Guid ReferenceId,
    string? Notes = null);

public sealed record FulfillReservationRequest(
    decimal? Quantity = null,
    string? Reference = null,
    string? Notes = null);
