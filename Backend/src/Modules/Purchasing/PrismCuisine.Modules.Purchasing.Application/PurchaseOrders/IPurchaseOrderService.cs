namespace PrismCuisine.Modules.Purchasing.Application.PurchaseOrders;

public interface IPurchaseOrderService
{
    Task<PurchaseOrderDto?> GetByIdAsync(int purchaseOrderId, CancellationToken cancellationToken = default);
    Task PostAsync(int purchaseOrderId, CancellationToken cancellationToken = default);
}
