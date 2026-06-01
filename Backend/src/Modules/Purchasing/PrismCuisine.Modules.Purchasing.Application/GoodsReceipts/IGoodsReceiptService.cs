namespace PrismCuisine.Modules.Purchasing.Application.GoodsReceipts;

public interface IGoodsReceiptService
{
    Task<IReadOnlyCollection<GoodsReceiptSummaryDto>> GetByPurchaseOrderIdAsync(
        int purchaseOrderId,
        CancellationToken cancellationToken = default);
    Task<GoodsReceiptDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<GoodsReceiptDto> CreateAsync(CreateGoodsReceiptRequest request, CancellationToken cancellationToken = default);
    Task AddLineAsync(int goodsReceiptId, AddGoodsReceiptLineRequest request, CancellationToken cancellationToken = default);
    Task<GoodsReceiptDto> PostAsync(int goodsReceiptId, CancellationToken cancellationToken = default);
}
