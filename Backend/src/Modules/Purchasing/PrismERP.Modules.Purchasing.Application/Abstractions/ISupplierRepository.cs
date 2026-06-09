using PrismERP.Modules.Purchasing.Domain.Entities;

namespace PrismERP.Modules.Purchasing.Application.Abstractions;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Supplier?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Supplier>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(Supplier supplier);
    void Update(Supplier supplier);
    Task<bool> IsExists(int id);
}
