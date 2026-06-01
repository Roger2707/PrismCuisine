using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Purchasing.Application.Abstractions;
using PrismCuisine.Modules.Purchasing.Domain.Entities;

namespace PrismCuisine.Modules.Purchasing.Infrastructure.Persistence.Repositories;

internal sealed class SupplierRepository(PrismCuisineDbContext db) : ISupplierRepository
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
}
