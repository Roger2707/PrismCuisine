using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Application.Abstractions.Persistence;

public interface IProductCategoryRepository
{
    Task<ProductCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductCategory?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(ProductCategory category);
    void Update(ProductCategory category);
}
