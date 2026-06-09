using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(PrismERPDbContext db) : IProductRepository
{
    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Product?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default) =>
        db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default) =>
        db.Products.AsNoTracking().FirstOrDefaultAsync(
            p => p.Sku == sku.Trim().ToUpperInvariant(),
            cancellationToken);

    public Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
        db.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyCollection<Product>)t.Result, cancellationToken);

    public void Add(Product product) => db.Products.Add(product);

    public void Update(Product product) => db.Products.Update(product);
}
