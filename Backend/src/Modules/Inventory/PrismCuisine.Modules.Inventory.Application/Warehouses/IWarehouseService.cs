namespace PrismCuisine.Modules.Inventory.Application.Warehouses;

public interface IWarehouseService
{
    Task<IReadOnlyCollection<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WarehouseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<WarehouseDto> CreateAsync(CreateWarehouseRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateWarehouseRequest request, CancellationToken cancellationToken = default);
}
