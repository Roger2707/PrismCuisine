namespace PrismERP.Modules.Purchasing.Application.GoodsReceipts;

public interface IGoodsReceiptService
{
    Task<IReadOnlyCollection<GoodsReceiptSummaryDto>> GetByPurchaseOrderIdAsync(int purchaseOrderId, CancellationToken cancellationToken = default);
    Task<GoodsReceiptDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    Task<GoodsReceiptDto> CreateAsync(CreateGoodsReceiptRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int goodsReceiptId, UpdateGoodsReceiptRequest request, CancellationToken cancellationToken = default);

    Task<GoodsReceiptDto> PostAsync(int goodsReceiptId, CancellationToken cancellationToken = default);
    Task CancelAsync(int goodsReceiptId, CancellationToken cancellationToken = default);
}
