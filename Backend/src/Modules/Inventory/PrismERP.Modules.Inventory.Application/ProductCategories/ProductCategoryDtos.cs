namespace PrismERP.Modules.Inventory.Application.ProductCategories;

public sealed record ProductCategoryDto(
    int Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive);

public sealed record CreateProductCategoryRequest(string Code, string Name, string? Description);

public sealed record UpdateProductCategoryRequest(string Name, string? Description);
