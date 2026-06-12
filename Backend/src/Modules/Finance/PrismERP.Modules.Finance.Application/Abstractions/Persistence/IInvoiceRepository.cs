using PrismERP.Modules.Finance.Domain.Entities;

namespace PrismERP.Modules.Finance.Application.Abstractions.Persistence;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Invoice>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(Invoice invoice);
    void Update(Invoice invoice);
}
