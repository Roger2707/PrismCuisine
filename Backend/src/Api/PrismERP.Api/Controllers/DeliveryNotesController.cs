using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Identity.Application.Authorization;
using PrismERP.Modules.Identity.Infrastructure.Auth.Authrizations;
using PrismERP.Modules.SalesOrdering.Application.Deliveries;

namespace PrismERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/sales-ordering/delivery-notes")]
public sealed class DeliveryNotesController(IDeliveryNoteService deliveryNoteService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(PermissionCodes.DeliveryRead)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var deliveries = await deliveryNoteService.GetAllAsync(cancellationToken);
        return Ok(deliveries);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(PermissionCodes.DeliveryRead)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var delivery = await deliveryNoteService.GetByIdAsync(id, cancellationToken);
        return delivery is null ? NotFound() : Ok(delivery);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.DeliveryWrite)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDeliveryNoteRequest request,
        CancellationToken cancellationToken)
    {
        var delivery = await deliveryNoteService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = delivery.Id }, delivery);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(PermissionCodes.DeliveryWrite)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateDeliveryNoteRequest request,
        CancellationToken cancellationToken)
    {
        await deliveryNoteService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/post")]
    [RequirePermission(PermissionCodes.DeliveryPost)]
    public async Task<IActionResult> Post(int id, CancellationToken cancellationToken)
    {
        await deliveryNoteService.PostAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/cancel")]
    [RequirePermission(PermissionCodes.DeliveryCancel)]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await deliveryNoteService.CancelAsync(id, cancellationToken);
        return NoContent();
    }
}
