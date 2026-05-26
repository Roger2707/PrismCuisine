using PrismCuisine.BuildingBlocks.Application.Abstractions.Cqrs;

namespace PrismCuisine.Modules.Purchasing.Application.PurchaseOrders.Commands.PostPurchaseOrder;

public sealed record PostPurchaseOrderCommand(Guid PurchaseOrderId) : ICommand;
