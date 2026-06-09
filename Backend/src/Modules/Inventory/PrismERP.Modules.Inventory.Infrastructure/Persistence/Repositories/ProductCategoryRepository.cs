using Microsoft.EntityFrameworkCore;
using PrismERP.BuildingBlocks.Infrastructure.Persistence;
using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class ProductCategoryRepository(PrismERPDbContext db) : IProductCategoryRepository
{
    public Task<ProductCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.ProductCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<ProductCategory?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        db.ProductCategories.FirstOrDefaultAsync(
            c => c.Code == code.Trim().ToUpperInvariant(),
            cancellationToken);

    public Task<IReadOnlyCollection<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default) =>
        db.ProductCategories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyCollection<ProductCategory>)t.Result, cancellationToken);

    public void Add(ProductCategory category) => db.ProductCategories.Add(category);

    public void Update(ProductCategory category) => db.ProductCategories.Update(category);
}
