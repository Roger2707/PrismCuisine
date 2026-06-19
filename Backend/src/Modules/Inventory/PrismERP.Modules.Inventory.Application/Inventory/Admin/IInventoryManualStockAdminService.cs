namespace PrismERP.Modules.Inventory.Application.Inventory.Admin;

/// <summary>
/// Manual stock operations via API and inventory seeder (receive / issue / adjust).
/// </summary>
public interface IInventoryManualStockAdminService
{
    Task<InventoryMovementDto> ReceiveAsync(ReceiveInventoryRequest request, CancellationToken cancellationToken = default);
    Task<List<InventoryMovementDto>> IssueAsync(IssueInventoryRequest request, CancellationToken cancellationToken = default);
    Task<List<InventoryMovementDto>> AdjustAsync(AdjustInventoryRequest request, CancellationToken cancellationToken = default);
}
