using PrismERP.BuildingBlocks.Domain.Exceptions;
using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Application.Inventory.Internal;
using PrismERP.Modules.Inventory.Domain.Entities;
using PrismERP.Modules.Inventory.Domain.Enums;

namespace PrismERP.Modules.Inventory.Application.Inventory.Workflows;

public sealed class InventorySalesReservationWorkflowService(
    IInventoryUnitOfWork unitOfWork,
    InventoryBalanceAccess balanceAccess,
    InventoryAvailabilityChecker availabilityChecker,
    InventoryFifoIssuer fifoIssuer) : IInventorySalesReservationWorkflowService
{
    public async Task<List<InventoryReservation>> ReserveForSalesOrderAsync(
        CreateReservationRequest reservationRequest,
        CancellationToken cancellationToken = default)
    {
        var reservations = new List<InventoryReservation>(reservationRequest.CreateReservationLines.Count);

        foreach (var request in reservationRequest.CreateReservationLines)
        {
            var balance = await balanceAccess.GetForUpdateByProductWarehouseAsync(
                request.ProductId,
                request.WarehouseId,
                cancellationToken);

            var existing = await unitOfWork.Reservations.GetActiveByReferenceAsync(
                InventoryReferenceType.SalesOrder,
                request.ReferenceId,
                cancellationToken);

            if (existing is not null)
            {
                throw new BusinessException("An active reservation already exists for this reference.");
            }

            await availabilityChecker.EnsureAvailableAsync(balance.Id, request.Quantity, cancellationToken);

            var reservation = InventoryReservation.Create(
                balance.Id,
                request.Quantity,
                InventoryReferenceType.SalesOrder,
                request.ReferenceId,
                request.Notes);

            unitOfWork.Reservations.Add(reservation);
            reservations.Add(reservation);
        }

        return reservations;
    }

    public Task<List<InventoryReservation>?> GetActivesByReferencesAsync(
        InventoryReferenceType referenceType,
        HashSet<int> referenceIds,
        CancellationToken cancellationToken = default) =>
        unitOfWork.Reservations.GetActivesByReferencesAsync(referenceType, referenceIds, cancellationToken);

    public async Task<List<InventoryMovement>> FulfillReservationsAsync(
        IReadOnlyList<FulfillReservationLine> lines,
        CancellationToken cancellationToken = default)
    {
        if (lines.Count == 0)
            return [];

        var balanceIds = lines.Select(l => l.Reservation.InventoryBalanceId).Distinct().ToList();
        var balances = await unitOfWork.Balances.GetByIdsForUpdateAsync(balanceIds, cancellationToken);
        var balanceById = balances.ToDictionary(b => b.Id);

        var allLayers = await unitOfWork.CostLayers.GetAvailableLayersForUpdateByBalanceIdsAsync(
            balanceIds,
            cancellationToken);
        var layersByBalance = allLayers
            .GroupBy(l => l.InventoryBalanceId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var movements = new List<InventoryMovement>(lines.Count);

        foreach (var line in lines)
        {
            var reservation = line.Reservation;
            if (line.Quantity > reservation.RemainingQuantity)
            {
                throw new BusinessException(
                    $"Fulfillment quantity '{line.Quantity}' exceeds remaining reservation '{reservation.RemainingQuantity}' for reference '{reservation.ReferenceId}'.");
            }

            reservation.RecordFulfillment(line.Quantity);
            unitOfWork.Reservations.Update(reservation);

            if (!balanceById.TryGetValue(reservation.InventoryBalanceId, out var balance))
            {
                throw new NotFoundException("Inventory balance for reservation was not found.");
            }

            if (!layersByBalance.TryGetValue(balance.Id, out var layers))
            {
                layers = [];
                layersByBalance[balance.Id] = layers;
            }

            var issued = fifoIssuer.IssueFromBalance(
                balance,
                line.Quantity,
                layers,
                InventoryReferenceType.SalesOrder,
                line.Reference ?? reservation.ReferenceId.ToString(),
                reservation.ReferenceId,
                line.Notes);

            if (issued.Count > 0)
                movements.AddRange(issued);
        }

        return movements;
    }

    public async Task ReturnDeliveryIssuesAsync(
        string deliveryNumber,
        IReadOnlyList<ReturnDeliveryLine> lines,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deliveryNumber))
        {
            throw new BusinessException("Delivery number is required.");
        }

        if (lines.Count == 0)
        {
            throw new BusinessException("At least one delivery line is required to return stock.");
        }

        var referenceIds = lines.Select(l => l.SalesOrderLineId).ToHashSet();
        var issueMovements = await unitOfWork.Movements.GetIssuesByDeliveryReferenceAsync(
            InventoryReferenceType.SalesOrder,
            deliveryNumber.Trim(),
            referenceIds,
            cancellationToken);

        if (issueMovements.Count == 0)
        {
            throw new BusinessException($"No issue movements found for delivery '{deliveryNumber}'.");
        }

        var movementsByLine = issueMovements
            .GroupBy(m => m.ReferenceId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var line in lines)
        {
            if (!movementsByLine.TryGetValue(line.SalesOrderLineId, out var lineMovements))
            {
                throw new BusinessException(
                    $"No issue movements found for sales order line '{line.SalesOrderLineId}' on delivery '{deliveryNumber}'.");
            }

            var issuedQty = lineMovements.Sum(m => m.Quantity);
            if (issuedQty != line.Quantity)
            {
                throw new BusinessException(
                    $"Return quantity '{line.Quantity}' does not match issued quantity '{issuedQty}' for sales order line '{line.SalesOrderLineId}'.");
            }
        }

        var layerIds = issueMovements
            .Select(m => m.InventoryCostLayerId)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        var layers = await unitOfWork.CostLayers.GetByIdsForUpdateAsync(layerIds, cancellationToken);
        var layerById = layers.ToDictionary(l => l.Id);

        var balanceIds = issueMovements.Select(m => m.InventoryBalanceId).Distinct().ToList();
        var balances = await unitOfWork.Balances.GetByIdsForUpdateAsync(balanceIds, cancellationToken);
        var balanceById = balances.ToDictionary(b => b.Id);

        var reservations = await unitOfWork.Reservations.GetByReferencesForUpdateAsync(
            InventoryReferenceType.SalesOrder,
            referenceIds,
            cancellationToken);
        var reservationByLine = reservations.ToDictionary(r => r.ReferenceId);

        foreach (var line in lines)
        {
            if (!reservationByLine.TryGetValue(line.SalesOrderLineId, out var reservation))
            {
                throw new BusinessException(
                    $"No reservation found for sales order line '{line.SalesOrderLineId}'.");
            }

            reservation.ReverseFulfillment(line.Quantity);
            unitOfWork.Reservations.Update(reservation);
        }

        foreach (var movement in issueMovements)
        {
            if (!layerById.TryGetValue(movement.InventoryCostLayerId, out var layer))
            {
                throw new NotFoundException($"Cost layer '{movement.InventoryCostLayerId}' was not found.");
            }

            if (!balanceById.TryGetValue(movement.InventoryBalanceId, out var balance))
            {
                throw new NotFoundException($"Inventory balance '{movement.InventoryBalanceId}' was not found.");
            }

            layer.Restore(movement.Quantity);
            unitOfWork.CostLayers.Update(layer);

            balance.Increase(movement.Quantity);
            unitOfWork.Balances.Update(balance);

            var returnMovement = InventoryMovement.Create(
                balance.Id,
                InventoryMovementType.Return,
                movement.Quantity,
                movement.UnitCost,
                InventoryReferenceType.SalesOrder,
                layer.Id,
                deliveryNumber.Trim(),
                movement.ReferenceId,
                $"Return from cancelled delivery {deliveryNumber.Trim()}");

            unitOfWork.Movements.Add(returnMovement);
        }
    }

    public async Task ReleaseReservationAsync(int reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await unitOfWork.Reservations.GetByIdForUpdateAsync(reservationId, cancellationToken)
            ?? throw new NotFoundException($"Reservation '{reservationId}' was not found.");

        reservation.Release();
        unitOfWork.Reservations.Update(reservation);
    }
}
