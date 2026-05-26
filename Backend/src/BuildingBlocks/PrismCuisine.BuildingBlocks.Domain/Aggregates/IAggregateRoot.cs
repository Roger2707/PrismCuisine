using PrismCuisine.BuildingBlocks.Domain.Events;

namespace PrismCuisine.BuildingBlocks.Domain.Aggregates;

public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
