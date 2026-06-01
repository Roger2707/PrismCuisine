namespace PrismCuisine.Modules.Purchasing.Application.PurchaseOrders;

public sealed record PurchaseOrderDto(
    int Id,
    string OrderNumber,
    string Status,
    DateTime? PostedAt,
    IReadOnlyList<PurchaseOrderLineDto> Lines);

public sealed record PurchaseOrderLineDto(int ProductId, decimal Quantity, decimal UnitPrice);
