using PrismERP.BuildingBlocks.Application.Abstractions.Persistence;

namespace PrismERP.Modules.Purchasing.Application.Abstractions;

public interface IPurchasingUnitOfWork : IUnitOfWork
{
    ISupplierRepository Suppliers { get; }
    IPurchaseOrderRepository PurchaseOrders { get; }
    IGoodsReceiptRepository GoodsReceipts { get; }
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default);
}
