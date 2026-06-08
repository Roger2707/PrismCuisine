using Microsoft.AspNetCore.Mvc;
using PrismCuisine.Modules.SalesOrdering.Application.Deliveries;

namespace PrismCuisine.Api.Controllers;

[ApiController]
[Route("api/sales-ordering/delivery-notes")]
public sealed class DeliveryNotesController(IDeliveryNoteService deliveryNoteService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var deliveries = await deliveryNoteService.GetAllAsync(cancellationToken);
        return Ok(deliveries);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var delivery = await deliveryNoteService.GetByIdAsync(id, cancellationToken);
        return delivery is null ? NotFound() : Ok(delivery);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDeliveryNoteRequest request,
        CancellationToken cancellationToken)
    {
        var delivery = await deliveryNoteService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = delivery.Id }, delivery);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateDeliveryNoteRequest request,
        CancellationToken cancellationToken)
    {
        await deliveryNoteService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/post")]
    public async Task<IActionResult> Post(int id, CancellationToken cancellationToken)
    {
        await deliveryNoteService.PostAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await deliveryNoteService.CancelAsync(id, cancellationToken);
        return NoContent();
    }
}
