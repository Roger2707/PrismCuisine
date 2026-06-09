namespace PrismERP.Modules.SalesOrdering.Application.Customers
{
    public interface ICustomerService
    {
        Task<IReadOnlyCollection<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<CustomerDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
        Task UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
        Task DeactivateAsync(int id, CancellationToken cancellationToken = default);
    }
}