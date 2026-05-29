namespace PrismCuisine.Modules.Inventory.Application.ProductCategories;

public interface IProductCategoryService
{
    Task<IReadOnlyCollection<ProductCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductCategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductCategoryDto> CreateAsync(CreateProductCategoryRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateProductCategoryRequest request, CancellationToken cancellationToken = default);
}
