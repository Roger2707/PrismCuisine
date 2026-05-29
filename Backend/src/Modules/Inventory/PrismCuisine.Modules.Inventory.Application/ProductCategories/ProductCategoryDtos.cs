namespace PrismCuisine.Modules.Inventory.Application.ProductCategories;

public sealed record ProductCategoryDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive);

public sealed record CreateProductCategoryRequest(string Code, string Name, string? Description);

public sealed record UpdateProductCategoryRequest(string Name, string? Description);
