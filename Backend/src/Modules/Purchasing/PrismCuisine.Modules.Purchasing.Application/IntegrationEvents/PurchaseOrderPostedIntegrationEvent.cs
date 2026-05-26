using PrismCuisine.BuildingBlocks.Application.Abstractions.Messaging;

namespace PrismCuisine.Modules.Purchasing.IntegrationEvents;

public sealed record PurchaseOrderPostedIntegrationEvent(
    Guid PurchaseOrderId,
    string OrderNumber) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => nameof(PurchaseOrderPostedIntegrationEvent);
}
