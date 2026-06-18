using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Finance.Application.Abstractions.Persistence;
using PrismERP.Modules.Finance.Domain.Entities;

namespace PrismERP.Modules.Finance.Infrastructure.Persistence.Repositories;

internal sealed class PaymentRepository(PrismERPDbContext db) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Payment?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default) =>
        db.Payments.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Payment?> GetByPaymentNumberAsync(string paymentNumber, CancellationToken cancellationToken = default) =>
        db.Payments.AsNoTracking().FirstOrDefaultAsync(
            p => p.PaymentNumber == paymentNumber.Trim().ToUpperInvariant(),
            cancellationToken);

    public Task<IReadOnlyCollection<Payment>> GetByInvoiceIdAsync(int invoiceId, CancellationToken cancellationToken = default) =>
        db.Payments
            .AsNoTracking()
            .Where(p => p.InvoiceId == invoiceId)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyCollection<Payment>)t.Result, cancellationToken);

    public Task<IReadOnlyCollection<Payment>> GetAllAsync(CancellationToken cancellationToken = default) =>
        db.Payments
            .AsNoTracking()
            .OrderBy(p => p.PaymentDate)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyCollection<Payment>)t.Result, cancellationToken);

    public void Add(Payment payment) => db.Payments.Add(payment);

    public void Update(Payment payment) => db.Payments.Update(payment);

    public Task<int> GetCountForDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return db.Payments.CountAsync(p => p.CreatedAt >= start && p.CreatedAt < end, cancellationToken);
    }
}
