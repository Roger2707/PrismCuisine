using PrismCuisine.BuildingBlocks.Domain.Events;

namespace PrismCuisine.Modules.Purchasing.Domain.Events;

public sealed record PurchaseOrderApprovedEvent(int PurchaseOrderId, string OrderNumber) : DomainEvent;
