using Microsoft.EntityFrameworkCore;
using PrismCuisine.BuildingBlocks.Infrastructure.Persistence;
using PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class ProductCategoryRepository(PrismCuisineDbContext db) : IProductCategoryRepository
{
    public Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
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
