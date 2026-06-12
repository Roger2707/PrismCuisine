using PrismERP.Modules.Finance.Domain.Entities;

namespace PrismERP.Modules.Finance.Application.Abstractions.Persistence;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByPaymentNumberAsync(string paymentNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Payment>> GetByInvoiceIdAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Payment>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(Payment payment);
    void Update(Payment payment);
}
