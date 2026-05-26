using PrismCuisine.BuildingBlocks.Application.Abstractions.Cqrs;
using PrismCuisine.Modules.Purchasing.Application.Abstractions;

namespace PrismCuisine.Modules.Purchasing.Application.PurchaseOrders.Queries.GetPurchaseOrderById;

public sealed class GetPurchaseOrderByIdQueryHandler(IPurchaseOrderRepository purchaseOrders)
    : IQueryHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDto?>
{
    public Task<PurchaseOrderDto?> Handle(
        GetPurchaseOrderByIdQuery request,
        CancellationToken cancellationToken) =>
        purchaseOrders.GetByIdWithLinesAsync(request.PurchaseOrderId, cancellationToken);
}
