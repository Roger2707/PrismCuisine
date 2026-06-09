using PrismERP.Modules.Inventory.Domain.Entities;
using PrismERP.Modules.Inventory.Domain.Enums;
using System.Threading.Tasks;

namespace PrismERP.Modules.Inventory.Application.Inventory;

public interface IInventoryPostingService
{
    Task<InventoryBalanceDto?> GetBalanceByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<InventoryBalanceDto?> GetBalanceAsync(int productId, int warehouseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryBalanceDto>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryMovementDto>> GetMovementsAsync(int balanceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryCostLayerDto>> GetCostLayersAsync(int balanceId, CancellationToken cancellationToken = default);
    Task<InventoryReservationDto?> GetReservationByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<InventoryReservation>?> GetActivesByReferencesAsync(
        InventoryReferenceType referenceType,
        HashSet<int> referenceIds,
        CancellationToken cancellationToken = default);

    Task<InventoryBalanceDto> EnsureBalanceAsync(
        CreateInventoryBalanceRequest request,
        CancellationToken cancellationToken = default);
    Task<InventoryMovementDto> ReceiveAsync(
        ReceiveInventoryRequest request,
        CancellationToken cancellationToken = default);
    Task<List<InventoryMovementDto>> IssueAsync(
        IssueInventoryRequest request,
        CancellationToken cancellationToken = default);
    Task<List<InventoryMovementDto>> AdjustAsync(
        AdjustInventoryRequest request,
        CancellationToken cancellationToken = default);
    Task<List<InventoryReservationDto>> ReserveAsync(
        CreateReservationRequest reservationRequest,
        CancellationToken cancellationToken = default);
    Task ReleaseReservationAsync(int reservationId, CancellationToken cancellationToken = default);

    Task<List<InventoryMovement>> FulfillReservationsAsync(
        IReadOnlyList<FulfillReservationLine> lines,
        CancellationToken cancellationToken = default);
    Task ReturnDeliveryIssuesAsync(
        string deliveryNumber,
        IReadOnlyList<ReturnDeliveryLine> lines,
        CancellationToken cancellationToken = default);
}
