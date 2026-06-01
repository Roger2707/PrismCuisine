using Microsoft.AspNetCore.Mvc;
using PrismCuisine.Modules.Purchasing.Application.PurchaseOrders;

namespace PrismCuisine.Api.Controllers;

[ApiController]
[Route("api/purchasing/purchase-orders")]
public sealed class PurchaseOrdersController(IPurchaseOrderService purchaseOrderService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await purchaseOrderService.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(int id, CancellationToken cancellationToken)
    {
        await purchaseOrderService.PostAsync(id, cancellationToken);
        return NoContent();
    }
}
