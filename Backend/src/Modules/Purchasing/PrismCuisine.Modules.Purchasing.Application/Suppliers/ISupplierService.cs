namespace PrismCuisine.Modules.Purchasing.Application.Suppliers;

public interface ISupplierService
{
    Task<IReadOnlyCollection<SupplierDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SupplierDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SupplierDto> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateSupplierRequest request, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int id, CancellationToken cancellationToken = default);
}
