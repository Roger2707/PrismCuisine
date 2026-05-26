using MassTransit;
using PrismCuisine.BuildingBlocks.Application.Abstractions.Messaging;

namespace PrismCuisine.BuildingBlocks.Infrastructure.Messaging;

public sealed class IntegrationEventPublisher(IPublishEndpoint publishEndpoint) : IIntegrationEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent =>
        publishEndpoint.Publish(integrationEvent, cancellationToken);
}
