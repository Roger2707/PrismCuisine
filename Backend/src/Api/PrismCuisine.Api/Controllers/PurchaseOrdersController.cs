using MediatR;
using Microsoft.AspNetCore.Mvc;
using PrismCuisine.Modules.Purchasing.Application.PurchaseOrders.Commands.PostPurchaseOrder;
using PrismCuisine.Modules.Purchasing.Application.PurchaseOrders.Queries.GetPurchaseOrderById;

namespace PrismCuisine.Api.Controllers;

[ApiController]
[Route("api/purchasing/purchase-orders")]
public sealed class PurchaseOrdersController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await sender.Send(new GetPurchaseOrderByIdQuery(id), cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{id:guid}/post")]
    public async Task<IActionResult> Post(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new PostPurchaseOrderCommand(id), cancellationToken);
        return NoContent();
    }
}
