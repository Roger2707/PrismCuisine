using PrismERP.Modules.Inventory.Application.Abstractions.Persistence;
using PrismERP.Modules.Inventory.Application.Inventory.Mapping;
using PrismERP.Modules.Inventory.Application.Inventory.Workflows;

namespace PrismERP.Modules.Inventory.Application.Inventory.Admin;

public sealed class InventoryReservationAdminService(
    IInventoryUnitOfWork unitOfWork,
    IInventorySalesReservationWorkflowService salesReservationWorkflow) : IInventoryReservationAdminService
{
    public async Task<List<InventoryReservationDto>> ReserveAsync(
        CreateReservationRequest reservationRequest,
        CancellationToken cancellationToken = default)
    {
        var reservations = await salesReservationWorkflow.ReserveForSalesOrderAsync(reservationRequest, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return reservations.Select(InventoryDtoMapper.ToReservationDto).ToList();
    }

    public async Task ReleaseReservationAsync(int reservationId, CancellationToken cancellationToken = default)
    {
        await salesReservationWorkflow.ReleaseReservationAsync(reservationId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
