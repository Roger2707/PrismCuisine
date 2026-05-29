using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.Inventory.Application.Abstractions.Persistence;
using PrismCuisine.Modules.Inventory.Domain.Entities;
using PrismCuisine.Modules.Inventory.Domain.Enums;
using PrismCuisine.Modules.Inventory.Domain.Services;

namespace PrismCuisine.Modules.Inventory.Application.Inventory;

public sealed class InventoryPostingService(IInventoryUnitOfWork unitOfWork) : IInventoryPostingService
{
    public async Task<InventoryBalanceDto?> GetBalanceByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var balance = await unitOfWork.Balances.GetByIdAsync(id, cancellationToken);
        return balance is null ? null : await MapBalanceAsync(balance, cancellationToken);
    }

    public async Task<InventoryBalanceDto?> GetBalanceAsync(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken = default)
    {
        var balance = await unitOfWork.Balances.GetByProductAndWarehouseAsync(productId, warehouseId, cancellationToken);
        return balance is null ? null : await MapBalanceAsync(balance, cancellationToken);
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

    public async Task<IReadOnlyCollection<InventoryMovementDto>> GetMovementsAsync(
        Guid balanceId,
        CancellationToken cancellationToken = default)
    {
        var movements = await unitOfWork.Movements.GetByBalanceIdAsync(balanceId, cancellationToken);
        return movements.Select(MapMovement).ToList();
    }

    public async Task<IReadOnlyCollection<InventoryCostLayerDto>> GetCostLayersAsync(
        Guid balanceId,
        CancellationToken cancellationToken = default)
    {
        var layers = await unitOfWork.CostLayers.GetAvailableLayersForUpdateAsync(balanceId, cancellationToken);
        return layers.Select(MapLayer).ToList();
    }

    public async Task<InventoryReservationDto?> GetReservationByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var reservation = await unitOfWork.Reservations.GetByIdForUpdateAsync(id, cancellationToken);
        return reservation is null ? null : MapReservation(reservation);
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

        balance.Increase(request.Quantity);
        unitOfWork.Balances.Update(balance);

        var movement = InventoryMovement.Create(
            balance.Id,
            InventoryMovementType.Receipt,
            request.Quantity,
            request.UnitCost,
            InventoryReferenceType.Manual,
            request.Reference,
            request.ReferenceId,
            request.Notes);

        unitOfWork.Movements.Add(movement);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapMovement(movement);
    }

    public async Task<InventoryMovementDto> IssueAsync(
        IssueInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var balance = await GetBalanceForUpdateByProductWarehouseAsync(
            request.ProductId,
            request.WarehouseId,
            cancellationToken);

        await EnsureAvailableAsync(balance.Id, request.Quantity, cancellationToken);

        var movement = await IssueFromBalanceAsync(
            balance,
            request.Quantity,
            InventoryReferenceType.Manual,
            request.Reference,
            request.ReferenceId,
            request.Notes,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapMovement(movement);
    }

    public async Task<InventoryMovementDto> AdjustAsync(
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

        InventoryMovement movement;

        if (delta > 0)
        {
            var layer = InventoryCostLayer.Create(balance.Id, delta, request.UnitCostForIncrease);
            unitOfWork.CostLayers.Add(layer);
            balance.Increase(delta);
            unitOfWork.Balances.Update(balance);

            movement = InventoryMovement.Create(
                balance.Id,
                InventoryMovementType.Adjustment,
                delta,
                request.UnitCostForIncrease,
                InventoryReferenceType.Adjustment,
                request.Reference,
                notes: request.Notes);
        }
        else
        {
            var issueQty = Math.Abs(delta);
            await EnsureAvailableAsync(balance.Id, issueQty, cancellationToken);
            movement = await IssueFromBalanceAsync(
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
            unitOfWork.Movements.Add(movement);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapMovement(movement);
    }

    public async Task<InventoryReservationDto> ReserveAsync(
        CreateReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        var balance = await GetBalanceForUpdateByProductWarehouseAsync(
            request.ProductId,
            request.WarehouseId,
            cancellationToken);

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
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapReservation(reservation);
    }

    public async Task ReleaseReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await unitOfWork.Reservations.GetByIdForUpdateAsync(reservationId, cancellationToken)
            ?? throw new DomainException($"Reservation '{reservationId}' was not found.");

        reservation.Release();
        unitOfWork.Reservations.Update(reservation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<InventoryMovementDto> FulfillReservationAsync(
        Guid reservationId,
        FulfillReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        var reservation = await unitOfWork.Reservations.GetByIdForUpdateAsync(reservationId, cancellationToken)
            ?? throw new DomainException($"Reservation '{reservationId}' was not found.");

        var fulfillQty = request.Quantity ?? reservation.RemainingQuantity;
        reservation.RecordFulfillment(fulfillQty);
        unitOfWork.Reservations.Update(reservation);

        var balance = await unitOfWork.Balances.GetByIdForUpdateAsync(reservation.InventoryBalanceId, cancellationToken)
            ?? throw new DomainException("Inventory balance for reservation was not found.");

        var movement = await IssueFromBalanceAsync(
            balance,
            fulfillQty,
            InventoryReferenceType.SalesOrder,
            request.Reference ?? reservation.ReferenceId.ToString(),
            reservation.ReferenceId,
            request.Notes,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapMovement(movement);
    }

    private async Task<InventoryMovement> IssueFromBalanceAsync(
        InventoryBalance balance,
        decimal quantity,
        InventoryReferenceType referenceType,
        string? reference,
        Guid? referenceId,
        string? notes,
        CancellationToken cancellationToken)
    {
        var layers = await unitOfWork.CostLayers.GetAvailableLayersForUpdateAsync(balance.Id, cancellationToken);
        var consumptions = FifoCosting.Consume(layers, quantity);

        var consumedLayerIds = consumptions.Select(c => c.CostLayerId).ToHashSet();
        foreach (var layer in layers.Where(l => consumedLayerIds.Contains(l.Id)))
        {
            unitOfWork.CostLayers.Update(layer);
        }

        var unitCost = FifoCosting.CalculateWeightedUnitCost(consumptions);
        balance.Decrease(quantity);
        unitOfWork.Balances.Update(balance);

        var movement = InventoryMovement.Create(
            balance.Id,
            InventoryMovementType.Issue,
            quantity,
            unitCost,
            referenceType,
            reference,
            referenceId,
            notes);

        unitOfWork.Movements.Add(movement);
        return movement;
    }

    private async Task EnsureAvailableAsync(
        Guid balanceId,
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

    private async Task<InventoryBalance> GetOrCreateBalanceForUpdateAsync(
        Guid productId,
        Guid warehouseId,
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
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        var balance = await unitOfWork.Balances.GetByProductAndWarehouseAsync(productId, warehouseId, cancellationToken)
            ?? throw new DomainException(
                $"No inventory balance for product '{productId}' at warehouse '{warehouseId}'. Create balance first.");

        return await unitOfWork.Balances.GetByIdForUpdateAsync(balance.Id, cancellationToken)
            ?? throw new DomainException("Inventory balance was not found.");
    }

    private async Task EnsureProductAndWarehouseExistAsync(
        Guid productId,
        Guid warehouseId,
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
}
