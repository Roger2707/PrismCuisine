namespace PrismCuisine.Modules.Inventory.Application.Warehouses;

public interface IWarehouseService
{
    Task<IReadOnlyCollection<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WarehouseDto> CreateAsync(CreateWarehouseRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateWarehouseRequest request, CancellationToken cancellationToken = default);
}
