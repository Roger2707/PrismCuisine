using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Finance.Application.Abstractions.Persistence;
using PrismERP.Modules.Finance.Domain.Entities;

namespace PrismERP.Modules.Finance.Infrastructure.Persistence.Repositories;

internal sealed class InvoiceLineRepository(PrismERPDbContext db) : IInvoiceLineRepository
{
    public Task<InvoiceLine?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.InvoiceLines.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public Task<IReadOnlyCollection<InvoiceLine>> GetByInvoiceIdAsync(int invoiceId, CancellationToken cancellationToken = default) =>
        db.InvoiceLines
            .AsNoTracking()
            .Where(l => l.InvoiceId == invoiceId)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyCollection<InvoiceLine>)t.Result, cancellationToken);

    public void Add(InvoiceLine line) => db.InvoiceLines.Add(line);

    public void Update(InvoiceLine line) => db.InvoiceLines.Update(line);

    public void Delete(InvoiceLine line) => db.InvoiceLines.Remove(line);
}
