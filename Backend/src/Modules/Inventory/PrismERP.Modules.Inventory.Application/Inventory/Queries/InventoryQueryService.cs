using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Application.Inventory.Mapping;

namespace PrismERP.Modules.Inventory.Application.Inventory.Queries;

public sealed class InventoryQueryService(IInventoryUnitOfWork unitOfWork) : IInventoryQueryService
{
    public async Task<InventoryBalanceDto?> GetBalanceByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var balance = await unitOfWork.Balances.GetByIdAsync(id, cancellationToken);
        return balance is null ? null : await MapBalanceAsync(balance, cancellationToken);
    }

    public async Task<InventoryBalanceDto?> GetBalanceAsync(
        int productId,
        int warehouseId,
        CancellationToken cancellationToken = default)
    {
        var balance = await unitOfWork.Balances.GetByProductAndWarehouseAsync(productId, warehouseId, cancellationToken);
        return balance is null ? null : await MapBalanceAsync(balance, cancellationToken);
    }

    public async Task<IReadOnlyCollection<InventoryBalanceDto>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        var balances = await unitOfWork.Balances.GetLowStockAsync(cancellationToken);
        var result = new List<InventoryBalanceDto>(balances.Count);

        foreach (var balance in balances)
        {
            result.Add(await MapBalanceAsync(balance, cancellationToken));
        }

        return result;
    }

    public async Task<IReadOnlyCollection<InventoryMovementDto>> GetMovementsAsync(
        int balanceId,
        CancellationToken cancellationToken = default)
    {
        var movements = await unitOfWork.Movements.GetByBalanceIdAsync(balanceId, cancellationToken);
        return movements.Select(InventoryDtoMapper.ToMovementDto).ToList();
    }

    public async Task<IReadOnlyCollection<InventoryCostLayerDto>> GetCostLayersAsync(
        int balanceId,
        CancellationToken cancellationToken = default)
    {
        var layers = await unitOfWork.CostLayers.GetAvailableLayersForUpdateAsync(balanceId, cancellationToken);
        return layers.Select(InventoryDtoMapper.ToLayerDto).ToList();
    }

    public async Task<IReadOnlyCollection<InventoryReservationDto>> GetReservationsByBalanceIdAsync(
        int balanceId,
        CancellationToken cancellationToken = default)
    {
        var reservations = await unitOfWork.Reservations.GetByBalanceIdAsync(balanceId, cancellationToken);
        return reservations.Select(InventoryDtoMapper.ToReservationDto).ToList();
    }

    public async Task<InventoryReservationDto?> GetReservationByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var reservation = await unitOfWork.Reservations.GetByIdForUpdateAsync(id, cancellationToken);
        return reservation is null ? null : InventoryDtoMapper.ToReservationDto(reservation);
    }

    private async Task<InventoryBalanceDto> MapBalanceAsync(
        Domain.Entities.InventoryBalance balance,
        CancellationToken cancellationToken)
    {
        var reserved = await unitOfWork.Reservations.GetActiveReservedQuantityAsync(balance.Id, cancellationToken);
        return InventoryDtoMapper.ToBalanceDto(balance, reserved);
    }
}
