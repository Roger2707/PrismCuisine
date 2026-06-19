using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;

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
}
