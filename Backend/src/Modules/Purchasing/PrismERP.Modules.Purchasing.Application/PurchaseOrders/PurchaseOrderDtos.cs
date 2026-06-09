namespace PrismERP.Modules.Purchasing.Application.PurchaseOrders;

public sealed record PurchaseOrderSummaryDto(
    int Id,
    string OrderNumber,
    int SupplierId,
    int WarehouseId,
    string Status,
    int? AmendedFromPurchaseOrderId,
    DateTime? ApprovedAt,
    decimal TotalAmount);

public sealed record PurchaseOrderDto(
    int Id,
    string OrderNumber,
    int SupplierId,
    int WarehouseId,
    string Status,
    int? AmendedFromPurchaseOrderId,
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
    string? Notes,
    IReadOnlyList<CreatePurchaseOrderLineRequest> Lines);

public sealed record CreatePurchaseOrderLineRequest(
    int ProductId,
    decimal QuantityOrdered,
    decimal UnitPrice);

public sealed record AddPurchaseOrderLineRequest(
    int ProductId,
    decimal QuantityOrdered,
    decimal UnitPrice);

public sealed record UpdatePurchaseOrderRequest(
    int SupplierId,
    int WarehouseId,
    string? Notes,
    IReadOnlyList<CreatePurchaseOrderLineRequest> Lines);

public sealed record CreatePurchaseOrderAmendmentRequest(
    string? Notes,
    bool CopyRemainingLines = true,
    IReadOnlyList<CreatePurchaseOrderLineRequest>? Lines = null);
