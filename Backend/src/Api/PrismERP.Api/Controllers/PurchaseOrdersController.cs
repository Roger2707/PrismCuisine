using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Identity.Application.Authorization;
using PrismERP.Modules.Identity.Infrastructure.Auth.Authrizations;
using PrismERP.Modules.Purchasing.Application.PurchaseOrders;

namespace PrismERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/purchasing/purchase-orders")]
public sealed class PurchaseOrdersController(IPurchaseOrderService purchaseOrderService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(PermissionCodes.PurchaseRead)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var orders = await purchaseOrderService.GetAllAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(PermissionCodes.PurchaseRead)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await purchaseOrderService.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.PurchaseWrite)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await purchaseOrderService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionCodes.PurchaseWrite)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdatePurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        await purchaseOrderService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/amendment")]
    [RequirePermission(PermissionCodes.PurchaseAmend)]
    public async Task<IActionResult> CreateAmendment(
        int id,
        [FromBody] CreatePurchaseOrderAmendmentRequest request,
        CancellationToken cancellationToken)
    {
        var amendment = await purchaseOrderService.CreateAmendmentAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = amendment.Id }, amendment);
    }

    [HttpPost("{id:int}/approve")]
    [RequirePermission(PermissionCodes.PurchaseApprove)]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        await purchaseOrderService.ApproveAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/cancel")]
    [RequirePermission(PermissionCodes.PurchaseCancel)]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await purchaseOrderService.CancelAsync(id, cancellationToken);
        return NoContent();
    }
}
