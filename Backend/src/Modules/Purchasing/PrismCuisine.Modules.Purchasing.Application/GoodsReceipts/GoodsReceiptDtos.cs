namespace PrismCuisine.Modules.Purchasing.Application.GoodsReceipts;

public sealed record GoodsReceiptSummaryDto(
    int Id,
    string ReceiptNumber,
    int PurchaseOrderId,
    string Status,
    DateTime? PostedAt);

public sealed record GoodsReceiptDto(
    int Id,
    string ReceiptNumber,
    int PurchaseOrderId,
    string Status,
    DateTime? PostedAt,
    string? Notes,
    IReadOnlyList<GoodsReceiptLineDto> Lines);

public sealed record GoodsReceiptLineDto(
    int Id,
    int PurchaseOrderLineId,
    int ProductId,
    decimal Quantity,
    decimal UnitCost);

public sealed record CreateGoodsReceiptRequest(
    int PurchaseOrderId,
    string? Notes);

public sealed record AddGoodsReceiptLineRequest(
    int PurchaseOrderLineId,
    decimal Quantity,
    decimal? UnitCost);
