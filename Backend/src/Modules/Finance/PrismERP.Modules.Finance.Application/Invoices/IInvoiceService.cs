namespace PrismERP.Modules.Finance.Application.Invoices;

public interface IInvoiceService
{
    Task<IReadOnlyCollection<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
    Task<InvoiceDto> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task PostAsync(int id, CancellationToken cancellationToken = default);
    Task CancelAsync(int id, CancellationToken cancellationToken = default);
    Task AddLineAsync(int invoiceId, CreateInvoiceLineRequest request, CancellationToken cancellationToken = default);
    Task UpdateLineAsync(int invoiceId, int lineId, UpdateInvoiceLineRequest request, CancellationToken cancellationToken = default);
    Task RemoveLineAsync(int invoiceId, int lineId, CancellationToken cancellationToken = default);
    Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken);
}
