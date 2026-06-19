namespace PrismERP.Modules.Inventory.Application.Inventory.Admin;

/// <summary>
/// Setup balance master data — used by API and inventory seeder only.
/// </summary>
public interface IInventoryBalanceAdminService
{
    Task<InventoryBalanceDto> EnsureBalanceAsync(
        CreateInventoryBalanceRequest request,
        CancellationToken cancellationToken = default);
}
