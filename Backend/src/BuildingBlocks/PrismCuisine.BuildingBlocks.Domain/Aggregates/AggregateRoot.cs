using PrismCuisine.BuildingBlocks.Domain.Entities;
using PrismCuisine.BuildingBlocks.Domain.Events;

namespace PrismCuisine.BuildingBlocks.Domain.Aggregates;

public abstract class AggregateRoot : Entity, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
