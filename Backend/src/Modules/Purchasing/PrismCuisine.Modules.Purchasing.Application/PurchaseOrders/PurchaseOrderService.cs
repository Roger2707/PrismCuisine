using PrismCuisine.BuildingBlocks.Application.Abstractions.Messaging;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.Purchasing.Application.Abstractions;
using PrismCuisine.Modules.Purchasing.IntegrationEvents;

namespace PrismCuisine.Modules.Purchasing.Application.PurchaseOrders;

public sealed class PurchaseOrderService(
    IPurchasingUnitOfWork unitOfWork,
    IIntegrationEventPublisher eventPublisher) : IPurchaseOrderService
{
    public Task<PurchaseOrderDto?> GetByIdAsync(int purchaseOrderId, CancellationToken cancellationToken = default) =>
        unitOfWork.PurchaseOrders.GetByIdWithLinesAsync(purchaseOrderId, cancellationToken);

    public async Task PostAsync(int purchaseOrderId, CancellationToken cancellationToken = default)
    {
        var order = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(purchaseOrderId, cancellationToken);

        if (order is null)
        {
            throw new DomainException($"Purchase order '{purchaseOrderId}' was not found.");
        }

        order.Post();

        unitOfWork.PurchaseOrders.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new PurchaseOrderPostedIntegrationEvent(order.Id, order.OrderNumber),
            cancellationToken);
    }
}
