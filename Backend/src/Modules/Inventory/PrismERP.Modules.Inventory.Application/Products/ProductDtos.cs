namespace PrismERP.Modules.Inventory.Application.Products;

public sealed record ProductDto(
    int Id,
    int CategoryId,
    string Sku,
    string Name,
    string Unit,
    string? Description,
    bool IsActive);

public sealed record CreateProductRequest(
    int CategoryId,
    string Sku,
    string Name,
    string Unit,
    string? Description);

public sealed record UpdateProductRequest(
    int CategoryId,
    string Name,
    string Unit,
    string? Description);
