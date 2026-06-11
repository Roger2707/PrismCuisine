using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Application.ProductCategories;

public sealed class ProductCategoryService(IInventoryUnitOfWork unitOfWork) : IProductCategoryService
{
    public async Task<IReadOnlyCollection<ProductCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await unitOfWork.ProductCategories.GetAllAsync(cancellationToken);
        return categories.Select(Map).ToList();
    }

    public async Task<ProductCategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await unitOfWork.ProductCategories.GetByIdAsync(id, cancellationToken);
        return category is null ? null : Map(category);
    }

    public async Task<ProductCategoryDto> CreateAsync(
        CreateProductCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await unitOfWork.ProductCategories.GetByCodeAsync(request.Code, cancellationToken);
        if (existing is not null)
        {
            throw new ValidationException("code", $"Category code '{request.Code}' already exists.");
        }

        var category = ProductCategory.Create(request.Code, request.Name, request.Description);
        unitOfWork.ProductCategories.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(category);
    }

    public async Task UpdateAsync(
        int id,
        UpdateProductCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await unitOfWork.ProductCategories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Product category '{id}' was not found.");

        category.Update(request.Name, request.Description);
        unitOfWork.ProductCategories.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ProductCategoryDto Map(ProductCategory category) =>
        new(category.Id, category.Code, category.Name, category.Description, category.IsActive);
}
