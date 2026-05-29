using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;

public interface IProductCategoryRepository
{
    Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductCategory?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(ProductCategory category);
    void Update(ProductCategory category);
}
