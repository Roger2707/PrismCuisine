using Microsoft.AspNetCore.Mvc;
using PrismCuisine.Modules.Inventory.Application.Inventory;

namespace PrismCuisine.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController(IInventoryPostingService inventoryPostingService) : ControllerBase
{
    [HttpGet("balances/low-stock")]
    public async Task<IActionResult> GetLowStock(CancellationToken cancellationToken)
    {
        var balances = await inventoryPostingService.GetLowStockAsync(cancellationToken);
        return Ok(balances);
    }

    [HttpGet("balances/{id:guid}")]
    public async Task<IActionResult> GetBalanceById(Guid id, CancellationToken cancellationToken)
    {
        var balance = await inventoryPostingService.GetBalanceByIdAsync(id, cancellationToken);
        return balance is null ? NotFound() : Ok(balance);
    }

    [HttpGet("balances")]
    public async Task<IActionResult> GetBalance(
        [FromQuery] Guid productId,
        [FromQuery] Guid warehouseId,
        CancellationToken cancellationToken)
    {
        var balance = await inventoryPostingService.GetBalanceAsync(productId, warehouseId, cancellationToken);
        return balance is null ? NotFound() : Ok(balance);
    }

    [HttpPost("balances")]
    public async Task<IActionResult> EnsureBalance(
        [FromBody] CreateInventoryBalanceRequest request,
        CancellationToken cancellationToken)
    {
        var balance = await inventoryPostingService.EnsureBalanceAsync(request, cancellationToken);
        return Ok(balance);
    }

    [HttpGet("balances/{id:guid}/movements")]
    public async Task<IActionResult> GetMovements(Guid id, CancellationToken cancellationToken)
    {
        var movements = await inventoryPostingService.GetMovementsAsync(id, cancellationToken);
        return Ok(movements);
    }

    [HttpGet("balances/{id:guid}/cost-layers")]
    public async Task<IActionResult> GetCostLayers(Guid id, CancellationToken cancellationToken)
    {
        var layers = await inventoryPostingService.GetCostLayersAsync(id, cancellationToken);
        return Ok(layers);
    }

    [HttpPost("receive")]
    public async Task<IActionResult> Receive(
        [FromBody] ReceiveInventoryRequest request,
        CancellationToken cancellationToken)
    {
        var movement = await inventoryPostingService.ReceiveAsync(request, cancellationToken);
        return Ok(movement);
    }

    [HttpPost("issue")]
    public async Task<IActionResult> Issue(
        [FromBody] IssueInventoryRequest request,
        CancellationToken cancellationToken)
    {
        var movement = await inventoryPostingService.IssueAsync(request, cancellationToken);
        return Ok(movement);
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust(
        [FromBody] AdjustInventoryRequest request,
        CancellationToken cancellationToken)
    {
        var movement = await inventoryPostingService.AdjustAsync(request, cancellationToken);
        return Ok(movement);
    }

    [HttpGet("reservations/{id:guid}")]
    public async Task<IActionResult> GetReservation(Guid id, CancellationToken cancellationToken)
    {
        var reservation = await inventoryPostingService.GetReservationByIdAsync(id, cancellationToken);
        return reservation is null ? NotFound() : Ok(reservation);
    }

    [HttpPost("reservations")]
    public async Task<IActionResult> Reserve(
        [FromBody] CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var reservation = await inventoryPostingService.ReserveAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
    }

    [HttpPost("reservations/{id:guid}/release")]
    public async Task<IActionResult> ReleaseReservation(Guid id, CancellationToken cancellationToken)
    {
        await inventoryPostingService.ReleaseReservationAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("reservations/{id:guid}/fulfill")]
    public async Task<IActionResult> FulfillReservation(
        Guid id,
        [FromBody] FulfillReservationRequest request,
        CancellationToken cancellationToken)
    {
        var movement = await inventoryPostingService.FulfillReservationAsync(id, request, cancellationToken);
        return Ok(movement);
    }
}
