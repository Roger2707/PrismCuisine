using PrismERP.Modules.Inventory.Domain.Entities;

namespace PrismERP.Modules.Inventory.Application.Inventory.Workflows;

public interface IInventoryReceivingWorkflowService
{
    Task<InventoryMovement> ReceiveStockAsync(ReceiveInventoryRequest request, CancellationToken cancellationToken = default);
    Task ReturnGoodsReceiptAsync(string goodsReceiptNumber, IReadOnlyList<ReturnGoodsReceiptLine> lines, CancellationToken cancellationToken = default);
}
