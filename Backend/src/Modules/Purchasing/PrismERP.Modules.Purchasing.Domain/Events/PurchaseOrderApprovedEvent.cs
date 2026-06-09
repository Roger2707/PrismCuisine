using PrismERP.BuildingBlocks.Domain.Events;

namespace PrismERP.Modules.Purchasing.Domain.Events;

public sealed record PurchaseOrderApprovedEvent(int PurchaseOrderId, string OrderNumber) : DomainEvent;
