namespace PrismERP.BuildingBlocks.Application.Abstractions.Messaging;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}
