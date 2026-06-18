using PrismERP.BuildingBlocks.Application.Abstractions.Messaging;

namespace PrismERP.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// In-process event publisher (replaces RabbitMQ/MassTransit).
/// Wire to SignalR in a later step when domain events need real-time push.
/// </summary>
public sealed class IntegrationEventPublisher : IIntegrationEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent =>
        Task.CompletedTask;
}
