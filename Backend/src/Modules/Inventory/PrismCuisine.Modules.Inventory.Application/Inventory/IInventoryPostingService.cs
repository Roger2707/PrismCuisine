namespace PrismCuisine.Modules.Inventory.Application.Inventory;

public interface IInventoryPostingService
{
    Task<InventoryBalanceDto?> GetBalanceByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<InventoryBalanceDto?> GetBalanceAsync(
        int productId,
        int warehouseId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryBalanceDto>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryMovementDto>> GetMovementsAsync(
        int balanceId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryCostLayerDto>> GetCostLayersAsync(
        int balanceId,
        CancellationToken cancellationToken = default);
    Task<InventoryReservationDto?> GetReservationByIdAsync(
        int id,
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
    Task ReleaseReservationAsync(int reservationId, CancellationToken cancellationToken = default);
    Task<InventoryMovementDto> FulfillReservationAsync(
        int reservationId,
        FulfillReservationRequest request,
        CancellationToken cancellationToken = default);
}
