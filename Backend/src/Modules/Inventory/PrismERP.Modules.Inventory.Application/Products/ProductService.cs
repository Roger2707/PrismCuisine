using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Application.Products;

public sealed class ProductService(IInventoryUnitOfWork unitOfWork) : IProductService
{
    public async Task<IReadOnlyCollection<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await unitOfWork.Products.GetAllAsync(cancellationToken);
        return products.Select(Map).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        return product is null ? null : Map(product);
    }

    public async Task<ProductDto?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var product = await unitOfWork.Products.GetBySkuAsync(sku, cancellationToken);
        return product is null ? null : Map(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        _ = await unitOfWork.ProductCategories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new DomainException($"Product category '{request.CategoryId}' was not found.");

        var existing = await unitOfWork.Products.GetBySkuAsync(request.Sku, cancellationToken);
        if (existing is not null)
        {
            throw new DomainException($"Product SKU '{request.Sku}' already exists.");
        }

        var product = Product.Create(
            request.CategoryId,
            request.Sku,
            request.Name,
            request.Unit,
            request.Description);

        unitOfWork.Products.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(product);
    }

    public async Task UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        _ = await unitOfWork.ProductCategories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new DomainException($"Product category '{request.CategoryId}' was not found.");

        var product = await unitOfWork.Products.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new DomainException($"Product '{id}' was not found.");

        product.Update(request.CategoryId, request.Name, request.Unit, request.Description);
        unitOfWork.Products.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await unitOfWork.Products.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new DomainException($"Product '{id}' was not found.");

        product.Deactivate();
        unitOfWork.Products.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ProductDto Map(Product product) =>
        new(product.Id, product.CategoryId, product.Sku, product.Name, product.Unit, product.Description, product.IsActive);
}
