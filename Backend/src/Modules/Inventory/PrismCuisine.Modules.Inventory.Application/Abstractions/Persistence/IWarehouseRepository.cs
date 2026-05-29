using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;

public interface IWarehouseRepository
{
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Warehouse?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(Warehouse warehouse);
    void Update(Warehouse warehouse);
}
