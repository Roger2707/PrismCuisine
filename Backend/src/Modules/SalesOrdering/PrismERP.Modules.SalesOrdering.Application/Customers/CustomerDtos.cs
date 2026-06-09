namespace PrismERP.Modules.SalesOrdering.Application.Customers
{
    public sealed record CustomerDto(
        int Id,
        string Code,
        string Name,
        string? Phone,
        string? Email,
        string? Address,
        string? TaxCode,
        bool IsActive);

    public sealed record CreateCustomerRequest(
        string Code,
        string Name,
        string? Phone,
        string? Email,
        string? Address,
        string? TaxCode);

    public sealed record UpdateCustomerRequest(
        string Name,
        string? Phone,
        string? Email,
        string? Address,
        string? TaxCode);
}
