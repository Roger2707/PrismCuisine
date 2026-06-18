using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Inventory.Application.Inventory;

namespace PrismERP.Api.Controllers;

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

    [HttpGet("balances/{id:int}")]
    public async Task<IActionResult> GetBalanceById(int id, CancellationToken cancellationToken)
    {
        var balance = await inventoryPostingService.GetBalanceByIdAsync(id, cancellationToken);
        return balance is null ? NotFound() : Ok(balance);
    }

    [HttpGet("balances")]
    public async Task<IActionResult> GetBalance(
        [FromQuery] int productId,
        [FromQuery] int warehouseId,
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

    [HttpGet("balances/{id}/reservations")]
    public async Task<IActionResult> GetReservations(int id, CancellationToken cancellationToken)
    {
        var reservations = await inventoryPostingService.GetReservationsByBalanceIdAsync(id, cancellationToken);
        return Ok(reservations);
    }

    [HttpGet("balances/{id}/movements")]
    public async Task<IActionResult> GetMovements(int id, CancellationToken cancellationToken)
    {
        var movements = await inventoryPostingService.GetMovementsAsync(id, cancellationToken);
        return Ok(movements);
    }

    [HttpGet("balances/{id}/cost-layers")]
    public async Task<IActionResult> GetCostLayers(int id, CancellationToken cancellationToken)
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

    [HttpGet("reservations/{id}")]
    public async Task<IActionResult> GetReservation(int id, CancellationToken cancellationToken)
    {
        var reservation = await inventoryPostingService.GetReservationByIdAsync(id, cancellationToken);
        return reservation is null ? NotFound() : Ok(reservation);
    }

    [HttpPost("reservations")]
    public async Task<IActionResult> Reserve(
        [FromBody] CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var reservations = await inventoryPostingService.ReserveAsync(request, cancellationToken);
        return Ok(reservations);
    }

    [HttpPost("reservations/{id}/release")]
    public async Task<IActionResult> ReleaseReservation(int id, CancellationToken cancellationToken)
    {
        await inventoryPostingService.ReleaseReservationAsync(id, cancellationToken);
        return NoContent();
    }
}
