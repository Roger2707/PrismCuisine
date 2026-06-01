using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class WarehouseRepository(PrismCuisineDbContext db) : IWarehouseRepository
{
    public Task<Warehouse?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Warehouses.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public Task<Warehouse?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        db.Warehouses.FirstOrDefaultAsync(
            w => w.Code == code.Trim().ToUpperInvariant(),
            cancellationToken);

    public Task<IReadOnlyCollection<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default) =>
        db.Warehouses
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyCollection<Warehouse>)t.Result, cancellationToken);

    public void Add(Warehouse warehouse) => db.Warehouses.Add(warehouse);

    public void Update(Warehouse warehouse) => db.Warehouses.Update(warehouse);
}
