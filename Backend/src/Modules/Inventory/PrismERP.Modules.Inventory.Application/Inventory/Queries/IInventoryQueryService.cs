namespace PrismERP.Modules.Inventory.Application.Inventory.Queries;

public interface IInventoryQueryService
{
    Task<InventoryBalanceDto?> GetBalanceByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<InventoryBalanceDto?> GetBalanceAsync(int productId, int warehouseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryBalanceDto>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryMovementDto>> GetMovementsAsync(int balanceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryCostLayerDto>> GetCostLayersAsync(int balanceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryReservationDto>> GetReservationsByBalanceIdAsync(int balanceId, CancellationToken cancellationToken = default);
    Task<InventoryReservationDto?> GetReservationByIdAsync(int id, CancellationToken cancellationToken = default);
}
