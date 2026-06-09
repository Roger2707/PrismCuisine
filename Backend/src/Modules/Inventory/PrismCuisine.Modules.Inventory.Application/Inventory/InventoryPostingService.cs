using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Inventory.Domain.Entities;
using PrismCuisine.Modules.Inventory.Domain.Enums;
using PrismCuisine.Modules.Inventory.Domain.Services;

namespace PrismCuisine.Modules.Inventory.Application.Inventory;

public sealed class InventoryPostingService(IInventoryUnitOfWork unitOfWork) : IInventoryPostingService
{

    #region Balance

    public async Task<InventoryBalanceDto?> GetBalanceByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
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

    public async Task<InventoryBalanceDto> EnsureBalanceAsync(
        CreateInventoryBalanceRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureProductAndWarehouseExistAsync(request.ProductId, request.WarehouseId, cancellationToken);

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

    private async Task<InventoryBalance> GetOrCreateBalanceForUpdateAsync(
        int productId,
        int warehouseId,
        CancellationToken cancellationToken)
    {
        var balance = await unitOfWork.Balances.GetByProductAndWarehouseAsync(productId, warehouseId, cancellationToken);

        if (balance is not null)
        {
            return await unitOfWork.Balances.GetByIdForUpdateAsync(balance.Id, cancellationToken)
                ?? throw new DomainException("Inventory balance was not found.");
        }

        await EnsureProductAndWarehouseExistAsync(productId, warehouseId, cancellationToken);
        balance = InventoryBalance.Create(productId, warehouseId, 0m);
        unitOfWork.Balances.Add(balance);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await unitOfWork.Balances.GetByIdForUpdateAsync(balance.Id, cancellationToken)
            ?? throw new DomainException("Inventory balance was not found.");
    }

    private async Task<InventoryBalance> GetBalanceForUpdateByProductWarehouseAsync(
        int productId,
        int warehouseId,
        CancellationToken cancellationToken)
    {
        var balance = await unitOfWork.Balances.GetByProductAndWarehouseAsync(productId, warehouseId, cancellationToken)
            ?? throw new DomainException(
                $"No inventory balance for product '{productId}' at warehouse '{warehouseId}'. Create balance first.");

        return await unitOfWork.Balances.GetByIdForUpdateAsync(balance.Id, cancellationToken)
            ?? throw new DomainException("Inventory balance was not found.");
    }

    public async Task<IReadOnlyCollection<InventoryBalanceDto>> GetLowStockAsync(
        CancellationToken cancellationToken = default)
    {
        var balances = await unitOfWork.Balances.GetLowStockAsync(cancellationToken);
        var result = new List<InventoryBalanceDto>(balances.Count);

        foreach (var balance in balances)
        {
            result.Add(await MapBalanceAsync(balance, cancellationToken));
        }

        return result;
    }

    #endregion

    #region Movement (History Transaction)

    public async Task<IReadOnlyCollection<InventoryMovementDto>> GetMovementsAsync(
        int balanceId,
        CancellationToken cancellationToken = default)
    {
        var movements = await unitOfWork.Movements.GetByBalanceIdAsync(balanceId, cancellationToken);
        return movements.Select(MapMovement).ToList();
    }

    #endregion

    #region Cost Layers

    public async Task<IReadOnlyCollection<InventoryCostLayerDto>> GetCostLayersAsync(
        int balanceId,
        CancellationToken cancellationToken = default)
    {
        var layers = await unitOfWork.CostLayers.GetAvailableLayersForUpdateAsync(balanceId, cancellationToken);
        return layers.Select(MapLayer).ToList();
    }

    #endregion

    #region Reservation

    public async Task<InventoryReservationDto?> GetReservationByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var reservation = await unitOfWork.Reservations.GetByIdForUpdateAsync(id, cancellationToken);
        return reservation is null ? null : MapReservation(reservation);
    }

    public async Task<List<InventoryReservation>?> GetActivesByReferencesAsync(InventoryReferenceType referenceType, HashSet<int> referenceIds, CancellationToken cancellationToken = default)
    {
        return await unitOfWork.Reservations.GetActivesByReferencesAsync(referenceType, referenceIds, cancellationToken);
    }

    public async Task<List<InventoryReservationDto>> ReserveAsync(CreateReservationRequest reservationRequest, CancellationToken cancellationToken = default)
    {
        var reservations = new List<InventoryReservationDto>(reservationRequest.CreateReservationLines.Count);
        foreach (var request in reservationRequest.CreateReservationLines)
        {
            var balance = await GetBalanceForUpdateByProductWarehouseAsync(
                request.ProductId,
                request.WarehouseId,
                cancellationToken);

            // Prevent multiple active reservations for the SAME reference to avoid over-reserving
            var existing = await unitOfWork.Reservations.GetActiveByReferenceAsync(
                InventoryReferenceType.SalesOrder,
                request.ReferenceId,
                cancellationToken);

            if (existing is not null)
            {
                throw new DomainException("An active reservation already exists for this reference.");
            }

            await EnsureAvailableAsync(balance.Id, request.Quantity, cancellationToken);

            var reservation = InventoryReservation.Create(
                balance.Id,
                request.Quantity,
                InventoryReferenceType.SalesOrder,
                request.ReferenceId,
                request.Notes);

            unitOfWork.Reservations.Add(reservation);
            reservations.Add(MapReservation(reservation));
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return reservations;
    }

    public async Task<List<InventoryMovement>> FulfillReservationsAsync(IReadOnlyList<FulfillReservationLine> lines, CancellationToken cancellationToken)
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
                throw new DomainException(
                    $"Fulfillment quantity '{line.Quantity}' exceeds remaining reservation '{reservation.RemainingQuantity}' for reference '{reservation.ReferenceId}'.");
            }

            reservation.RecordFulfillment(line.Quantity);
            unitOfWork.Reservations.Update(reservation);

            if (!balanceById.TryGetValue(reservation.InventoryBalanceId, out var balance))
            {
                throw new DomainException("Inventory balance for reservation was not found.");
            }

            if (!layersByBalance.TryGetValue(balance.Id, out var layers))
            {
                layers = [];
                layersByBalance[balance.Id] = layers;
            }

            var movement = IssueFromBalance(
                balance,
                line.Quantity,
                layers,
                InventoryReferenceType.SalesOrder,
                line.Reference ?? reservation.ReferenceId.ToString(),
                reservation.ReferenceId,
                line.Notes);

            if (movement != null && movement.Count > 0)
                movements.AddRange(movement);
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
            throw new DomainException("Delivery number is required.");
        }

        if (lines.Count == 0)
        {
            throw new DomainException("At least one delivery line is required to return stock.");
        }

        var referenceIds = lines.Select(l => l.SalesOrderLineId).ToHashSet();
        var issueMovements = await unitOfWork.Movements.GetIssuesByDeliveryReferenceAsync(
            InventoryReferenceType.SalesOrder,
            deliveryNumber.Trim(),
            referenceIds,
            cancellationToken);

        if (issueMovements.Count == 0)
        {
            throw new DomainException($"No issue movements found for delivery '{deliveryNumber}'.");
        }

        var movementsByLine = issueMovements
            .GroupBy(m => m.ReferenceId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var line in lines)
        {
            if (!movementsByLine.TryGetValue(line.SalesOrderLineId, out var lineMovements))
            {
                throw new DomainException(
                    $"No issue movements found for sales order line '{line.SalesOrderLineId}' on delivery '{deliveryNumber}'.");
            }

            var issuedQty = lineMovements.Sum(m => m.Quantity);
            if (issuedQty != line.Quantity)
            {
                throw new DomainException(
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
                throw new DomainException(
                    $"No reservation found for sales order line '{line.SalesOrderLineId}'.");
            }

            reservation.ReverseFulfillment(line.Quantity);
            unitOfWork.Reservations.Update(reservation);
        }

        foreach (var movement in issueMovements)
        {
            if (!layerById.TryGetValue(movement.InventoryCostLayerId, out var layer))
            {
                throw new DomainException($"Cost layer '{movement.InventoryCostLayerId}' was not found.");
            }

            if (!balanceById.TryGetValue(movement.InventoryBalanceId, out var balance))
            {
                throw new DomainException($"Inventory balance '{movement.InventoryBalanceId}' was not found.");
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
            ?? throw new DomainException($"Reservation '{reservationId}' was not found.");

        reservation.Release();
        unitOfWork.Reservations.Update(reservation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Action Methods (Import / Export / Adjust)

    public async Task<InventoryMovementDto> ReceiveAsync(
        ReceiveInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var balance = await GetOrCreateBalanceForUpdateAsync(
            request.ProductId,
            request.WarehouseId,
            cancellationToken);

        var layer = InventoryCostLayer.Create(balance.Id, request.Quantity, request.UnitCost);
        unitOfWork.CostLayers.Add(layer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        balance.Increase(request.Quantity);
        unitOfWork.Balances.Update(balance);

        var movement = InventoryMovement.Create(
            balance.Id,
            InventoryMovementType.Receipt,
            request.Quantity,
            request.UnitCost,
            InventoryReferenceType.Manual,
            layer.Id,
            request.Reference,
            request.ReferenceId,
            request.Notes);

        unitOfWork.Movements.Add(movement);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapMovement(movement);
    }

    public async Task<List<InventoryMovementDto>> IssueAsync(
        IssueInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var balance = await GetBalanceForUpdateByProductWarehouseAsync(
            request.ProductId,
            request.WarehouseId,
            cancellationToken);

        await EnsureAvailableAsync(balance.Id, request.Quantity, cancellationToken);

        var movements = await IssueFromBalanceAsync(
            balance,
            request.Quantity,
            InventoryReferenceType.Manual,
            request.Reference,
            request.ReferenceId,
            request.Notes,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return movements.Select(MapMovement).ToList();
    }   

    private async Task<List<InventoryMovement>> IssueFromBalanceAsync(
        InventoryBalance balance,
        decimal quantity,
        InventoryReferenceType referenceType,
        string? reference,
        int? referenceId,
        string? notes,
        CancellationToken cancellationToken)
    {
        var layers = (await unitOfWork.CostLayers.GetAvailableLayersForUpdateAsync(balance.Id, cancellationToken)).ToList();
        return IssueFromBalance(balance, quantity, layers, referenceType, reference, referenceId, notes);
    }

    private List<InventoryMovement> IssueFromBalance(
        InventoryBalance balance,
        decimal quantity,
        List<InventoryCostLayer> layers,
        InventoryReferenceType referenceType,
        string? reference,
        int? referenceId,
        string? notes)
    {
        var movements = new List<InventoryMovement>();
        var consumptions = FifoCosting.Consume(layers, quantity);

        var consumedLayerIds = consumptions.Select(c => c.CostLayerId).ToHashSet();
        foreach (var layer in layers.Where(l => consumedLayerIds.Contains(l.Id)))
        {
            unitOfWork.CostLayers.Update(layer);
        }

        balance.Decrease(quantity);
        unitOfWork.Balances.Update(balance);

        foreach (var consumption in consumptions)
        {
            var layer = layers.First(l => l.Id == consumption.CostLayerId);
            var movement = InventoryMovement.Create(
                balance.Id,
                InventoryMovementType.Issue,
                consumption.Quantity,
                layer.UnitCost,
                referenceType,
                layer.Id,
                reference,
                referenceId,
                notes);

            unitOfWork.Movements.Add(movement);
            movements.Add(movement);
        }

        return movements;
    }

    public async Task<List<InventoryMovementDto>> AdjustAsync(
        AdjustInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var balance = await GetBalanceForUpdateByProductWarehouseAsync(
            request.ProductId,
            request.WarehouseId,
            cancellationToken);

        if (request.NewQuantity < 0)
        {
            throw new DomainException("Adjusted quantity cannot be negative.");
        }

        var delta = request.NewQuantity - balance.QuantityOnHand;
        if (delta == 0)
        {
            throw new DomainException("Adjusted quantity is the same as current on-hand quantity.");
        }

        List<InventoryMovement> movements = new();

        if (delta > 0)
        {
            var layer = InventoryCostLayer.Create(balance.Id, delta, request.UnitCostForIncrease);
            unitOfWork.CostLayers.Add(layer);
            balance.Increase(delta);
            unitOfWork.Balances.Update(balance);

            var movement = InventoryMovement.Create(
                balance.Id,
                InventoryMovementType.Adjustment,
                delta,
                request.UnitCostForIncrease,
                InventoryReferenceType.Adjustment,
                0,
                request.Reference,
                notes: request.Notes);

            movements.Add(movement);
        }
        else
        {
            var issueQty = Math.Abs(delta);
            await EnsureAvailableAsync(balance.Id, issueQty, cancellationToken);
            movements = await IssueFromBalanceAsync(
                balance,
                issueQty,
                InventoryReferenceType.Adjustment,
                request.Reference,
                referenceId: null,
                request.Notes,
                cancellationToken);
        }

        if (delta > 0)
        {
            foreach (var movement in movements)
            {
                unitOfWork.Movements.Add(movement);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return movements.Select(MapMovement).ToList();
    }

    #endregion

    #region Private Helper Methods

    private async Task EnsureAvailableAsync(
        int balanceId,
        decimal quantity,
        CancellationToken cancellationToken)
    {
        var balance = await unitOfWork.Balances.GetByIdAsync(balanceId, cancellationToken)
            ?? throw new DomainException("Inventory balance was not found.");

        var reserved = await unitOfWork.Reservations.GetActiveReservedQuantityAsync(balanceId, cancellationToken);
        var available = balance.GetAvailable(reserved);

        if (quantity > available)
        {
            throw new DomainException(
                $"Insufficient available quantity. On-hand: {balance.QuantityOnHand}, reserved: {reserved}, requested: {quantity}.");
        }
    }   

    private async Task EnsureProductAndWarehouseExistAsync(
        int productId,
        int warehouseId,
        CancellationToken cancellationToken)
    {
        _ = await unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new DomainException($"Product '{productId}' was not found.");

        _ = await unitOfWork.Warehouses.GetByIdAsync(warehouseId, cancellationToken)
            ?? throw new DomainException($"Warehouse '{warehouseId}' was not found.");
    }

    private async Task<InventoryBalanceDto> MapBalanceAsync(
        InventoryBalance balance,
        CancellationToken cancellationToken)
    {
        var reserved = await unitOfWork.Reservations.GetActiveReservedQuantityAsync(balance.Id, cancellationToken);
        return new InventoryBalanceDto(
            balance.Id,
            balance.ProductId,
            balance.WarehouseId,
            balance.QuantityOnHand,
            reserved,
            balance.GetAvailable(reserved),
            balance.ReorderLevel,
            balance.IsBelowReorderLevel());
    }

    private static InventoryMovementDto MapMovement(InventoryMovement movement) =>
        new(
            movement.Id,
            movement.InventoryBalanceId,
            movement.MovementType.ToString(),
            movement.Quantity,
            movement.UnitCost,
            movement.ReferenceType.ToString(),
            movement.Reference,
            movement.ReferenceId,
            movement.Notes,
            movement.CreatedAt);

    private static InventoryCostLayerDto MapLayer(InventoryCostLayer layer) =>
        new(
            layer.Id,
            layer.InventoryBalanceId,
            layer.QuantityReceived,
            layer.QuantityRemaining,
            layer.UnitCost,
            layer.ReceivedAt);

    private static InventoryReservationDto MapReservation(InventoryReservation reservation) =>
        new(
            reservation.Id,
            reservation.InventoryBalanceId,
            reservation.Quantity,
            reservation.FulfilledQuantity,
            reservation.RemainingQuantity,
            reservation.Status.ToString(),
            reservation.ReferenceType.ToString(),
            reservation.ReferenceId,
            reservation.Notes);

    #endregion

}
