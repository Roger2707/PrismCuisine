namespace PrismCuisine.Modules.Purchasing.Application.Suppliers;

public sealed record SupplierDto(
    int Id,
    string Code,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? TaxCode,
    bool IsActive);

public sealed record CreateSupplierRequest(
    string Code,
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? TaxCode);

public sealed record UpdateSupplierRequest(
    string Name,
    string? Phone,
    string? Email,
    string? Address,
    string? TaxCode);
