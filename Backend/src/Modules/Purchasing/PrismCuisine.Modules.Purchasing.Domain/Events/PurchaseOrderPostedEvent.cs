using PrismCuisine.BuildingBlocks.Domain.Events;

namespace PrismCuisine.Modules.Purchasing.Domain.Events;

public sealed record PurchaseOrderPostedEvent(int PurchaseOrderId, string OrderNumber) : DomainEvent;
