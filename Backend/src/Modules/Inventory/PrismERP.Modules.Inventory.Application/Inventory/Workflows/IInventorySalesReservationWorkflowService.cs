using PrismERP.Modules.Inventory.Domain.Entities;
using PrismERP.Modules.Inventory.Domain.Enums;

namespace PrismERP.Modules.Inventory.Application.Inventory.Workflows;

// DTOs: CreateReservationRequest, FulfillReservationLine, ReturnDeliveryLine in parent namespace

public interface IInventorySalesReservationWorkflowService
{
    Task<List<InventoryReservation>> ReserveForSalesOrderAsync(
        CreateReservationRequest reservationRequest,
        CancellationToken cancellationToken = default);

    Task<List<InventoryReservation>> GetActivesByReferencesAsync(
        InventoryReferenceType referenceType,
        HashSet<int> referenceIds,
        CancellationToken cancellationToken = default);

    Task<List<InventoryMovement>> FulfillReservationsAsync(
        IReadOnlyList<FulfillReservationLine> lines,
        CancellationToken cancellationToken = default);

    Task ReturnDeliveryIssuesAsync(
        string deliveryNumber,
        IReadOnlyList<ReturnDeliveryLine> lines,
        CancellationToken cancellationToken = default);

    Task ReleaseReservationAsync(int reservationId, CancellationToken cancellationToken = default);
    Task ReleaseReservationsAsync(HashSet<int> referenceIds, CancellationToken cancellationToken = default);
}
