namespace PrismERP.BuildingBlocks.Application.Abstractions.Messaging;

public interface IIntegrationEventPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent;
}
