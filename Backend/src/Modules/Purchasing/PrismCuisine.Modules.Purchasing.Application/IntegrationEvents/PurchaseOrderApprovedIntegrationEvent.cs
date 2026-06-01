using PrismCuisine.BuildingBlocks.Application.Abstractions.Messaging;

namespace PrismCuisine.Modules.Purchasing.IntegrationEvents;

public sealed record PurchaseOrderApprovedIntegrationEvent(
    int PurchaseOrderId,
    string OrderNumber) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => nameof(PurchaseOrderApprovedIntegrationEvent);
}
