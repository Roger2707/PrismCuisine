namespace PrismCuisine.Modules.Purchasing.Application.PurchaseOrders;

public interface IPurchaseOrderService
{
    Task<IReadOnlyCollection<PurchaseOrderSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PurchaseOrderDto?> GetByIdAsync(int purchaseOrderId, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int purchaseOrderId, UpdatePurchaseOrderRequest request, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDto> CreateAmendmentAsync(
        int purchaseOrderId,
        CreatePurchaseOrderAmendmentRequest request,
        CancellationToken cancellationToken = default);
    Task AddLineAsync(int purchaseOrderId, AddPurchaseOrderLineRequest request, CancellationToken cancellationToken = default);
    Task ApproveAsync(int purchaseOrderId, CancellationToken cancellationToken = default);
    Task CancelAsync(int purchaseOrderId, CancellationToken cancellationToken = default);
}
