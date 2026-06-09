using PrismERP.BuildingBlocks.Domain.Events;

namespace PrismERP.BuildingBlocks.Domain.Aggregates;

public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
