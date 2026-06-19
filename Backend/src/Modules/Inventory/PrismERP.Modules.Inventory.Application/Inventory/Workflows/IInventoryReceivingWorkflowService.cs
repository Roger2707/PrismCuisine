using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Application.Inventory.Workflows;

/// <summary>
/// Goods receipt post — caller owns SaveChanges inside purchasing transaction.
/// </summary>
public interface IInventoryReceivingWorkflowService
{
    Task<InventoryMovement> ReceiveStockAsync(
        ReceiveInventoryRequest request,
        CancellationToken cancellationToken = default);
}
