namespace PrismCuisine.Modules.Purchasing.Application.PurchaseOrders;

public sealed record PurchaseOrderSummaryDto(
    int Id,
    string OrderNumber,
    int SupplierId,
    int WarehouseId,
    string Status,
    DateTime? ApprovedAt,
    decimal TotalAmount);

public sealed record PurchaseOrderDto(
    int Id,
    string OrderNumber,
    int SupplierId,
    int WarehouseId,
    string Status,
    DateTime? ApprovedAt,
    string? Notes,
    IReadOnlyList<PurchaseOrderLineDto> Lines);

public sealed record PurchaseOrderLineDto(
    int Id,
    int ProductId,
    decimal QuantityOrdered,
    decimal QuantityReceived,
    decimal QuantityRemaining,
    decimal UnitPrice);

public sealed record CreatePurchaseOrderRequest(
    int SupplierId,
    int WarehouseId,
    string? Notes);

public sealed record AddPurchaseOrderLineRequest(
    int ProductId,
    decimal QuantityOrdered,
    decimal UnitPrice);
