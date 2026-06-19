using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Application.Inventory.Internal;
using PrismERP.Modules.Inventory.Application.Inventory.Mapping;
using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Application.Inventory.Admin;

public sealed class InventoryBalanceAdminService(
    IInventoryUnitOfWork unitOfWork,
    InventoryBalanceAccess balanceAccess) : IInventoryBalanceAdminService
{
    public async Task<InventoryBalanceDto> EnsureBalanceAsync(
        CreateInventoryBalanceRequest request,
        CancellationToken cancellationToken = default)
    {
        await balanceAccess.EnsureProductAndWarehouseExistAsync(request.ProductId, request.WarehouseId, cancellationToken);

        var existing = await unitOfWork.Balances.GetByProductAndWarehouseAsync(
            request.ProductId,
            request.WarehouseId,
            cancellationToken);

        if (existing is not null)
        {
            existing.SetReorderLevel(request.ReorderLevel);
            unitOfWork.Balances.Update(existing);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return await MapBalanceAsync(existing, cancellationToken);
        }

        var balance = InventoryBalance.Create(request.ProductId, request.WarehouseId, request.ReorderLevel);
        unitOfWork.Balances.Add(balance);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await MapBalanceAsync(balance, cancellationToken);
    }

    private async Task<InventoryBalanceDto> MapBalanceAsync(
        InventoryBalance balance,
        CancellationToken cancellationToken)
    {
        var reserved = await unitOfWork.Reservations.GetActiveReservedQuantityAsync(balance.Id, cancellationToken);
        return InventoryDtoMapper.ToBalanceDto(balance, reserved);
    }
}
