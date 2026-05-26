using PrismCuisine.BuildingBlocks.Domain.Events;

namespace PrismCuisine.Modules.Purchasing.Domain.Events;

public sealed record PurchaseOrderPostedEvent(Guid PurchaseOrderId, string OrderNumber) : DomainEvent;
