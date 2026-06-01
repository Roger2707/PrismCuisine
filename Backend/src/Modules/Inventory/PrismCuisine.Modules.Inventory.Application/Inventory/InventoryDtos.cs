namespace PrismCuisine.Modules.Inventory.Application.Inventory;

public sealed record InventoryBalanceDto(
    int Id,
    int ProductId,
    int WarehouseId,
    decimal QuantityOnHand,
    decimal ReservedQuantity,
    decimal AvailableQuantity,
    decimal ReorderLevel,
    bool IsBelowReorderLevel);

public sealed record InventoryMovementDto(
    int Id,
    int InventoryBalanceId,
    string MovementType,
    decimal Quantity,
    decimal UnitCost,
    string ReferenceType,
    string? Reference,
    int? ReferenceId,
    string? Notes,
    DateTime CreatedAt);

public sealed record InventoryCostLayerDto(
    int Id,
    int InventoryBalanceId,
    decimal QuantityReceived,
    decimal QuantityRemaining,
    decimal UnitCost,
    DateTime ReceivedAt);

public sealed record InventoryReservationDto(
    int Id,
    int InventoryBalanceId,
    decimal Quantity,
    decimal FulfilledQuantity,
    decimal RemainingQuantity,
    string Status,
    string ReferenceType,
    int ReferenceId,
    string? Notes);

public sealed record CreateInventoryBalanceRequest(
    int ProductId,
    int WarehouseId,
    decimal ReorderLevel);

public sealed record ReceiveInventoryRequest(
    int ProductId,
    int WarehouseId,
    decimal Quantity,
    decimal UnitCost,
    string? Reference = null,
    int? ReferenceId = null,
    string? Notes = null);

public sealed record IssueInventoryRequest(
    int ProductId,
    int WarehouseId,
    decimal Quantity,
    string? Reference = null,
    int? ReferenceId = null,
    string? Notes = null);

public sealed record AdjustInventoryRequest(
    int ProductId,
    int WarehouseId,
    decimal NewQuantity,
    decimal UnitCostForIncrease,
    string? Reference = null,
    string? Notes = null);

public sealed record CreateReservationRequest(
    int ProductId,
    int WarehouseId,
    decimal Quantity,
    int ReferenceId,
    string? Notes = null);

public sealed record FulfillReservationRequest(
    decimal? Quantity = null,
    string? Reference = null,
    string? Notes = null);
