using PrismCuisine.BuildingBlocks.Application.Abstractions.Persistence;

namespace PrismCuisine.Modules.Purchasing.Application.Abstractions;

public interface IPurchasingUnitOfWork : IUnitOfWork
{
    ISupplierRepository Suppliers { get; }
    IPurchaseOrderRepository PurchaseOrders { get; }
    IGoodsReceiptRepository GoodsReceipts { get; }
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default);
}
