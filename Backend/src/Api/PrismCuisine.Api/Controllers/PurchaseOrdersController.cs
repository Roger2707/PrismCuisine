using Microsoft.AspNetCore.Mvc;
using PrismCuisine.Modules.Purchasing.Application.PurchaseOrders;

namespace PrismCuisine.Api.Controllers;

[ApiController]
[Route("api/purchasing/purchase-orders")]
public sealed class PurchaseOrdersController(IPurchaseOrderService purchaseOrderService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var orders = await purchaseOrderService.GetAllAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await purchaseOrderService.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await purchaseOrderService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPost("{id:int}/lines")]
    public async Task<IActionResult> AddLine(
        int id,
        [FromBody] AddPurchaseOrderLineRequest request,
        CancellationToken cancellationToken)
    {
        await purchaseOrderService.AddLineAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        await purchaseOrderService.ApproveAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await purchaseOrderService.CancelAsync(id, cancellationToken);
        return NoContent();
    }
}
