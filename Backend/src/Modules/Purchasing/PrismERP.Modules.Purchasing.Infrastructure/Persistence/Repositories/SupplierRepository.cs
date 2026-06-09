using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Purchasing.Application.Abstractions;
using PrismERP.Modules.Purchasing.Domain.Entities;

namespace PrismERP.Modules.Purchasing.Infrastructure.Persistence.Repositories;

internal sealed class SupplierRepository(PrismERPDbContext db) : ISupplierRepository
{
    public Task<Supplier?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Suppliers.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Supplier?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        db.Suppliers.FirstOrDefaultAsync(s => s.Code == code.Trim().ToUpperInvariant(), cancellationToken);

    public async Task<IReadOnlyCollection<Supplier>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

    public void Add(Supplier supplier) => db.Suppliers.Add(supplier);

    public void Update(Supplier supplier) => db.Suppliers.Update(supplier);
    public async Task<bool> IsExists(int id) => await db.Suppliers.AnyAsync(s => s.Id == id);
}
