namespace PrismERP.Modules.Purchasing.Application.PurchaseInvoices;

public interface IPurchaseInvoiceService
{
    Task<IReadOnlyCollection<PurchaseInvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PurchaseInvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PurchaseInvoiceDto> CreateAsync(CreatePurchaseInvoiceRequest request, CancellationToken cancellationToken = default);
}
