using PrismCuisine.BuildingBlocks.Application.Abstractions.Persistence;

namespace PrismCuisine.Modules.Purchasing.Application.Abstractions;

public interface IPurchasingUnitOfWork : IUnitOfWork
{
    IPurchaseOrderRepository PurchaseOrders { get; }
}
