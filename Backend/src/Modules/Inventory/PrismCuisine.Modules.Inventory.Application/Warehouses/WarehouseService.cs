using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Inventory.Domain.Entities;

namespace PrismCuisine.Modules.Inventory.Application.Warehouses;

public sealed class WarehouseService(IInventoryUnitOfWork unitOfWork) : IWarehouseService
{
    public async Task<IReadOnlyCollection<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var warehouses = await unitOfWork.Warehouses.GetAllAsync(cancellationToken);
        return warehouses.Select(Map).ToList();
    }

    public async Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await unitOfWork.Warehouses.GetByIdAsync(id, cancellationToken);
        return warehouse is null ? null : Map(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(
        CreateWarehouseRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await unitOfWork.Warehouses.GetByCodeAsync(request.Code, cancellationToken);
        if (existing is not null)
        {
            throw new DomainException($"Warehouse code '{request.Code}' already exists.");
        }

        var warehouse = Warehouse.Create(request.Code, request.Name, request.Location);
        unitOfWork.Warehouses.Add(warehouse);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(warehouse);
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateWarehouseRequest request,
        CancellationToken cancellationToken = default)
    {
        var warehouse = await unitOfWork.Warehouses.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException($"Warehouse '{id}' was not found.");

        warehouse.Update(request.Name, request.Location);
        unitOfWork.Warehouses.Update(warehouse);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static WarehouseDto Map(Warehouse warehouse) =>
        new(warehouse.Id, warehouse.Code, warehouse.Name, warehouse.Location, warehouse.IsActive);
}
