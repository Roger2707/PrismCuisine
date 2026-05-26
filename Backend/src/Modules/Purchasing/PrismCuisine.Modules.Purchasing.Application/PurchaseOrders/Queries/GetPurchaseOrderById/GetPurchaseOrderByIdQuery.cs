using PrismCuisine.BuildingBlocks.Application.Abstractions.Cqrs;

namespace PrismCuisine.Modules.Purchasing.Application.PurchaseOrders.Queries.GetPurchaseOrderById;

public sealed record GetPurchaseOrderByIdQuery(Guid PurchaseOrderId) : IQuery<PurchaseOrderDto?>;

public sealed record PurchaseOrderDto(
    Guid Id,
    string OrderNumber,
    string Status,
    DateTime? PostedAt,
    IReadOnlyList<PurchaseOrderLineDto> Lines);

public sealed record PurchaseOrderLineDto(Guid ProductId, decimal Quantity, decimal UnitPrice);
