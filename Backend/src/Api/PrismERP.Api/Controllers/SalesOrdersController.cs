using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.SalesOrdering.Application.SalesOrders;

namespace PrismERP.Api.Controllers;

[ApiController]
[Route("api/sales-ordering/sales-orders")]
public sealed class SalesOrdersController(ISalesOrderService salesOrderService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var orders = await salesOrderService.GetAllAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await salesOrderService.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSalesOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await salesOrderService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateSalesOrderRequest request,
        CancellationToken cancellationToken)
    {
        await salesOrderService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        await salesOrderService.ApproveAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await salesOrderService.CancelAsync(id, cancellationToken);
        return NoContent();
    }
}
