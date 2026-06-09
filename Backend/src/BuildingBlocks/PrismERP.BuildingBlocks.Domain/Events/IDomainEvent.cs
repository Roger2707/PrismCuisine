namespace PrismERP.BuildingBlocks.Domain.Events;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
