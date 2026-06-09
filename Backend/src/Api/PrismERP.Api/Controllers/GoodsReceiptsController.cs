using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Purchasing.Application.GoodsReceipts;

namespace PrismERP.Api.Controllers;

[ApiController]
[Route("api/purchasing/goods-receipts")]
public sealed class GoodsReceiptsController(IGoodsReceiptService goodsReceiptService) : ControllerBase
{
    [HttpGet("by-purchase-order/{purchaseOrderId:int}")]
    public async Task<IActionResult> GetByPurchaseOrder(int purchaseOrderId, CancellationToken cancellationToken)
    {
        var receipts = await goodsReceiptService.GetByPurchaseOrderIdAsync(purchaseOrderId, cancellationToken);
        return Ok(receipts);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var receipt = await goodsReceiptService.GetByIdAsync(id, cancellationToken);
        return receipt is null ? NotFound() : Ok(receipt);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateGoodsReceiptRequest request,
        CancellationToken cancellationToken)
    {
        var receipt = await goodsReceiptService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = receipt.Id }, receipt);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateGoodsReceiptRequest request,
        CancellationToken cancellationToken)
    {
        await goodsReceiptService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/lines")]
    public async Task<IActionResult> AddLine(
        int id,
        [FromBody] AddGoodsReceiptLineRequest request,
        CancellationToken cancellationToken)
    {
        await goodsReceiptService.AddLineAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/post")]
    public async Task<IActionResult> Post(int id, CancellationToken cancellationToken)
    {
        var receipt = await goodsReceiptService.PostAsync(id, cancellationToken);
        return Ok(receipt);
    }
}
