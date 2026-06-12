using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Finance.Application.Abstractions.Persistence;
using PrismERP.Modules.Finance.Domain.Entities;

namespace PrismERP.Modules.Finance.Infrastructure.Persistence.Repositories;

internal sealed class InvoiceRepository(PrismERPDbContext db) : IInvoiceRepository
{
    public Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Invoices
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public Task<Invoice?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default) =>
        db.Invoices
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default) =>
        db.Invoices
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                i => i.InvoiceNumber == invoiceNumber.Trim().ToUpperInvariant(),
                cancellationToken);

    public Task<IReadOnlyCollection<Invoice>> GetAllAsync(CancellationToken cancellationToken = default) =>
        db.Invoices
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .AsNoTracking()
            .OrderBy(i => i.InvoiceDate)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyCollection<Invoice>)t.Result, cancellationToken);

    public void Add(Invoice invoice) => db.Invoices.Add(invoice);

    public void Update(Invoice invoice) => db.Invoices.Update(invoice);

    public Task<int> GetCountForDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return db.SalesOrders.CountAsync(o => o.CreatedAt >= start && o.CreatedAt < end, cancellationToken);
    }
}
