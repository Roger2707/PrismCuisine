namespace PrismCuisine.Modules.Inventory.Application.Products;

public sealed record ProductDto(
    Guid Id,
    Guid CategoryId,
    string Sku,
    string Name,
    string Unit,
    string? Description,
    bool IsActive);

public sealed record CreateProductRequest(
    Guid CategoryId,
    string Sku,
    string Name,
    string Unit,
    string? Description);

public sealed record UpdateProductRequest(
    Guid CategoryId,
    string Name,
    string Unit,
    string? Description);
