using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Finance.Application.Abstractions.Persistence;
using PrismERP.Modules.Finance.Infrastructure.Persistence.Repositories;

namespace PrismERP.Modules.Finance.Infrastructure.Persistence;

internal sealed class FinanceUnitOfWork(PrismERPDbContext db) : IFinanceUnitOfWork
{
    public IInvoiceRepository Invoices { get; } = new InvoiceRepository(db);
    public IPaymentRepository Payments { get; } = new PaymentRepository(db);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
