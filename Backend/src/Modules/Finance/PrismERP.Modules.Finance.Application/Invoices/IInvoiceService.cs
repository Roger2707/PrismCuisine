namespace PrismERP.Modules.Finance.Application.Invoices;

public interface IInvoiceService
{
    Task<IReadOnlyCollection<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InvoiceDto>> GetInvoicesByPurchaseOrderAsync(int purchaseOrderId, CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetByGoodsReceiptIdAsync(int goodsReceiptId, CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetByDeliveryNoteIdAsync(int deliveryNoteId, CancellationToken cancellationToken = default);
    Task<InvoiceDto> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task CancelAsync(int id, CancellationToken cancellationToken = default);
    Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken);
}
