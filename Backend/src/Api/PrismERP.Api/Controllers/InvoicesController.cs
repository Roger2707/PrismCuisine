using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Finance.Application.Invoices;
using PrismERP.Modules.Identity.Application.Authorization;
using PrismERP.Modules.Identity.Infrastructure.Auth.Authrizations;

namespace PrismERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/finance/invoices")]
public sealed class InvoicesController(IInvoiceService invoiceService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(PermissionCodes.InvoiceRead)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var invoices = await invoiceService.GetAllAsync(cancellationToken);
        return Ok(invoices);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(PermissionCodes.InvoiceRead)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.GetByIdAsync(id, cancellationToken);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpGet("by-delivery-note/{deliveryNoteId:int}")]
    [RequirePermission(PermissionCodes.InvoiceRead)]
    public async Task<IActionResult> GetByDeliveryNote(int deliveryNoteId, CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.GetByDeliveryNoteIdAsync(deliveryNoteId, cancellationToken);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpGet("by-goods-receipt/{goodsReceiptId:int}")]
    [RequirePermission(PermissionCodes.InvoiceRead)]
    public async Task<IActionResult> GetByGoodsReceipt(int goodsReceiptId, CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.GetByGoodsReceiptIdAsync(goodsReceiptId, cancellationToken);
        return invoice is null ? NotFound() : Ok(invoice);
    }
}
