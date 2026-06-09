using PrismERP.Modules.Purchasing.Application.GoodsReceipts;
using PrismERP.Modules.Purchasing.Domain.Entities;

namespace PrismERP.Modules.Purchasing.Application.Abstractions;

public interface IGoodsReceiptRepository
{
    Task<GoodsReceipt?> GetByIdWithLinesForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<GoodsReceiptDto?> GetByIdWithLinesAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<GoodsReceiptSummaryDto>> GetByPurchaseOrderIdAsync(
        int purchaseOrderId,
        CancellationToken cancellationToken = default);
    Task<int> GetCountForDateAsync(DateTime date, CancellationToken cancellationToken = default);
    void Add(GoodsReceipt receipt);
    void Update(GoodsReceipt receipt);
}
