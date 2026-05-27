namespace PrismCuisine.Modules.Purchasing.Application.PurchaseOrders;

public sealed record PurchaseOrderDto(
    Guid Id,
    string OrderNumber,
    string Status,
    DateTime? PostedAt,
    IReadOnlyList<PurchaseOrderLineDto> Lines);

public sealed record PurchaseOrderLineDto(Guid ProductId, decimal Quantity, decimal UnitPrice);
