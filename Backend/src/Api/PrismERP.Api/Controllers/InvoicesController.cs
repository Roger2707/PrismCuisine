using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Finance.Application.Invoices;

namespace PrismERP.Api.Controllers;

[ApiController]
[Route("api/finance/invoices")]
public sealed class InvoicesController(IInvoiceService invoiceService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var invoices = await invoiceService.GetAllAsync(cancellationToken);
        return Ok(invoices);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.GetByIdAsync(id, cancellationToken);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpGet("by-delivery-note/{deliveryNoteId:int}")]
    public async Task<IActionResult> GetByDeliveryNote(int deliveryNoteId, CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.GetByDeliveryNoteIdAsync(deliveryNoteId, cancellationToken);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpGet("by-goods-receipt/{goodsReceiptId:int}")]
    public async Task<IActionResult> GetByGoodsReceipt(int goodsReceiptId, CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.GetByGoodsReceiptIdAsync(goodsReceiptId, cancellationToken);
        return invoice is null ? NotFound() : Ok(invoice);
    }
}
