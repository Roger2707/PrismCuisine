namespace PrismERP.Modules.Inventory.Application.Products;

public interface IProductService
{
    Task<IReadOnlyCollection<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int id, CancellationToken cancellationToken = default);
}
