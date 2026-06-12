using PrismERP.Modules.Finance.Domain.Entities;

namespace PrismERP.Modules.Finance.Application.Abstractions.Persistence;

public interface IInvoiceLineRepository
{
    Task<InvoiceLine?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InvoiceLine>> GetByInvoiceIdAsync(int invoiceId, CancellationToken cancellationToken = default);
    void Add(InvoiceLine line);
    void Update(InvoiceLine line);
    void Delete(InvoiceLine line);
}
