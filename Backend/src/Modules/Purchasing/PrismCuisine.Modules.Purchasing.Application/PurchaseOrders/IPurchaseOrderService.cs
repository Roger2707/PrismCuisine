namespace PrismCuisine.Modules.Purchasing.Application.PurchaseOrders;

public interface IPurchaseOrderService
{
    Task<PurchaseOrderDto?> GetByIdAsync(Guid purchaseOrderId, CancellationToken cancellationToken = default);
    Task PostAsync(Guid purchaseOrderId, CancellationToken cancellationToken = default);
}
