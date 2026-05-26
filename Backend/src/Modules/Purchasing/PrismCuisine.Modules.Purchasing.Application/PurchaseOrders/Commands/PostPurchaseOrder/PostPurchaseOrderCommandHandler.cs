using PrismCuisine.BuildingBlocks.Application.Abstractions.Cqrs;
using PrismCuisine.BuildingBlocks.Application.Abstractions.Messaging;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.Purchasing.Application.Abstractions;
using PrismCuisine.Modules.Purchasing.IntegrationEvents;

namespace PrismCuisine.Modules.Purchasing.Application.PurchaseOrders.Commands.PostPurchaseOrder;

public sealed class PostPurchaseOrderCommandHandler(
    IPurchasingUnitOfWork unitOfWork,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<PostPurchaseOrderCommand>
{
    public async Task Handle(PostPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.PurchaseOrders.GetByIdWithLinesForUpdateAsync(
            request.PurchaseOrderId,
            cancellationToken);

        if (order is null)
        {
            throw new DomainException($"Purchase order '{request.PurchaseOrderId}' was not found.");
        }

        order.Post();

        unitOfWork.PurchaseOrders.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new PurchaseOrderPostedIntegrationEvent(order.Id, order.OrderNumber),
            cancellationToken);
    }
}
