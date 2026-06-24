using PrismERP.Modules.Finance.Application.Invoices;
using PrismERP.Modules.Finance.Domain.Entities;

namespace PrismERP.Modules.Finance.Application.Abstractions.Persistence;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByGoodsReceiptIdAsync(int goodsReceiptId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByDeliveryNoteIdAsync(int deliveryNoteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Invoice>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Invoice>> GetByPurchaseOrderAsync(int purchaseOrderId, CancellationToken cancellationToken = default);
    void Add(Invoice invoice);
    void Update(Invoice invoice);
    void Delete(Invoice invoice);
    Task<int> GetCountForDateAsync(DateTime date, CancellationToken cancellationToken = default);
}
