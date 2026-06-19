using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Application.Inventory.Internal;

public sealed class InventoryBalanceAccess(IInventoryUnitOfWork unitOfWork)
{
    public async Task EnsureProductAndWarehouseExistAsync(
        int productId,
        int warehouseId,
        CancellationToken cancellationToken)
    {
        _ = await unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException($"Product '{productId}' was not found.");

        _ = await unitOfWork.Warehouses.GetByIdAsync(warehouseId, cancellationToken)
            ?? throw new NotFoundException($"Warehouse '{warehouseId}' was not found.");
    }

    public async Task<InventoryBalance> GetForUpdateByProductWarehouseAsync(
        int productId,
        int warehouseId,
        CancellationToken cancellationToken)
    {
        var balance = await unitOfWork.Balances.GetByProductAndWarehouseAsync(productId, warehouseId, cancellationToken)
            ?? throw new NotFoundException(
                $"No inventory balance for product '{productId}' at warehouse '{warehouseId}'. Create balance first.");

        return await unitOfWork.Balances.GetByIdForUpdateAsync(balance.Id, cancellationToken)
            ?? throw new NotFoundException("Inventory balance was not found.");
    }

    public async Task<InventoryBalance> GetOrCreateForUpdateAsync(
        int productId,
        int warehouseId,
        CancellationToken cancellationToken)
    {
        var balance = await unitOfWork.Balances.GetByProductAndWarehouseAsync(productId, warehouseId, cancellationToken);

        if (balance is not null)
        {
            return await unitOfWork.Balances.GetByIdForUpdateAsync(balance.Id, cancellationToken)
                ?? throw new NotFoundException("Inventory balance was not found.");
        }

        await EnsureProductAndWarehouseExistAsync(productId, warehouseId, cancellationToken);
        balance = InventoryBalance.Create(productId, warehouseId, 0m);
        unitOfWork.Balances.Add(balance);

        return balance;
    }
}
