using MediatR;

namespace PrismCuisine.BuildingBlocks.Domain.Events;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
