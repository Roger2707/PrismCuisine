namespace PrismCuisine.Modules.Inventory.Application.Inventory;

public interface IInventoryPostingService
{
    Task<InventoryBalanceDto?> GetBalanceByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InventoryBalanceDto?> GetBalanceAsync(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryBalanceDto>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryMovementDto>> GetMovementsAsync(
        Guid balanceId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryCostLayerDto>> GetCostLayersAsync(
        Guid balanceId,
        CancellationToken cancellationToken = default);
    Task<InventoryReservationDto?> GetReservationByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<InventoryBalanceDto> EnsureBalanceAsync(
        CreateInventoryBalanceRequest request,
        CancellationToken cancellationToken = default);
    Task<InventoryMovementDto> ReceiveAsync(
        ReceiveInventoryRequest request,
        CancellationToken cancellationToken = default);
    Task<InventoryMovementDto> IssueAsync(
        IssueInventoryRequest request,
        CancellationToken cancellationToken = default);
    Task<InventoryMovementDto> AdjustAsync(
        AdjustInventoryRequest request,
        CancellationToken cancellationToken = default);
    Task<InventoryReservationDto> ReserveAsync(
        CreateReservationRequest request,
        CancellationToken cancellationToken = default);
    Task ReleaseReservationAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<InventoryMovementDto> FulfillReservationAsync(
        Guid reservationId,
        FulfillReservationRequest request,
        CancellationToken cancellationToken = default);
}
