namespace PrismCuisine.Modules.Inventory.Application.ProductCategories;

public interface IProductCategoryService
{
    Task<IReadOnlyCollection<ProductCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductCategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductCategoryDto> CreateAsync(CreateProductCategoryRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateProductCategoryRequest request, CancellationToken cancellationToken = default);
}
