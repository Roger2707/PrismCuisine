using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Application.Inventory.Internal;

public sealed class InventoryAvailabilityChecker(IInventoryUnitOfWork unitOfWork)
{
    public async Task EnsureAvailableAsync(
        int balanceId,
        decimal quantity,
        CancellationToken cancellationToken)
    {
        var balance = await unitOfWork.Balances.GetByIdAsync(balanceId, cancellationToken)
            ?? throw new NotFoundException("Inventory balance was not found.");

        var reserved = await unitOfWork.Reservations.GetActiveReservedQuantityAsync(balanceId, cancellationToken);
        var available = balance.GetAvailable(reserved);

        if (quantity > available)
        {
            throw new BusinessException(
                $"Insufficient available quantity. On-hand: {balance.QuantityOnHand}, reserved: {reserved}, requested: {quantity}.");
        }
    }

    public async Task EnsureAvailablesAsync(
        List<(InventoryBalance balance, decimal requestedQty)> request, CancellationToken cancellationToken)
    {
        var balanceIds = request.Select(r => r.balance.Id).ToHashSet();
        var balance_reservedQty = await unitOfWork.Reservations.GetActiveReservedQuantityBalancesAsync(balanceIds, cancellationToken);

        foreach(var item in request)
        {
            var balance = item.balance;
            decimal reservedQty = balance_reservedQty[balance.Id];
            decimal available = balance.GetAvailable(reservedQty);

            if (item.requestedQty > available)
            {
                throw new BusinessException(
                    $"Insufficient available quantity. On-hand: {balance.QuantityOnHand}, reserved: {reservedQty}, requested: {item.requestedQty}.");
            }
        }

    }
}
