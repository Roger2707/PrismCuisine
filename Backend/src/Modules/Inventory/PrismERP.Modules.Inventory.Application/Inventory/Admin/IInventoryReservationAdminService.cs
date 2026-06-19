namespace PrismERP.Modules.Inventory.Application.Inventory.Admin;

/// <summary>
/// Manual reservation API (POST reserve / release) — not used by sales order workflow.
/// </summary>
public interface IInventoryReservationAdminService
{
    Task<List<InventoryReservationDto>> ReserveAsync(
        CreateReservationRequest reservationRequest,
        CancellationToken cancellationToken = default);

    Task ReleaseReservationAsync(int reservationId, CancellationToken cancellationToken = default);
}
