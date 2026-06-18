using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Purchasing.Application.PurchaseInvoices;

namespace PrismERP.Api.Controllers;

[ApiController]
[Route("api/purchasing/purchase-invoices")]
public sealed class PurchaseInvoicesController(IPurchaseInvoiceService purchaseInvoiceService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var invoices = await purchaseInvoiceService.GetAllAsync(cancellationToken);
        return Ok(invoices);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var invoice = await purchaseInvoiceService.GetByIdAsync(id, cancellationToken);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpGet("by-goods-receipt/{goodsReceiptId:int}")]
    public async Task<IActionResult> GetByGoodsReceipt(int goodsReceiptId, CancellationToken cancellationToken)
    {
        var invoice = await purchaseInvoiceService.GetByGoodsReceiptIdAsync(goodsReceiptId, cancellationToken);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFromGoodsReceipt(
        [FromBody] CreatePurchaseInvoiceFromGoodsReceiptRequest request,
        CancellationToken cancellationToken)
    {
        var invoice = await purchaseInvoiceService.CreateFromGoodsReceiptAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
    }
}
