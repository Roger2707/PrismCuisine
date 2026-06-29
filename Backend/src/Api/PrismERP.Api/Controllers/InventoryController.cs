using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Identity.Application.Authorization;
using PrismERP.Modules.Identity.Infrastructure.Auth.Authrizations;
using PrismERP.Modules.Inventory.Application.Inventory;
using PrismERP.Modules.Inventory.Application.Inventory.Admin;
using PrismERP.Modules.Inventory.Application.Inventory.Queries;

namespace PrismERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory")]
public sealed class InventoryController(
    IInventoryQueryService queryService,
    IInventoryBalanceAdminService balanceAdminService,
    IInventoryManualStockAdminService manualStockAdminService,
    IInventoryReservationAdminService reservationAdminService) : ControllerBase
{
    [HttpGet("balances/low-stock")]
    [RequirePermission(PermissionCodes.InventoryRead)]
    public async Task<IActionResult> GetLowStock(CancellationToken cancellationToken)
    {
        var balances = await queryService.GetLowStockAsync(cancellationToken);
        return Ok(balances);
    }

    [HttpGet("balances/{id:int}")]
    [RequirePermission(PermissionCodes.InventoryRead)]
    public async Task<IActionResult> GetBalanceById(int id, CancellationToken cancellationToken)
    {
        var balance = await queryService.GetBalanceByIdAsync(id, cancellationToken);
        return balance is null ? NotFound() : Ok(balance);
    }

    [HttpGet("balances")]
    [RequirePermission(PermissionCodes.InventoryRead)]
    public async Task<IActionResult> GetBalance(
        [FromQuery] int productId,
        [FromQuery] int warehouseId,
        CancellationToken cancellationToken)
    {
        var balance = await queryService.GetBalanceAsync(productId, warehouseId, cancellationToken);
        return balance is null ? NotFound() : Ok(balance);
    }

    [HttpPost("balances")]
    [RequirePermission(PermissionCodes.InventoryAdjust)]
    public async Task<IActionResult> EnsureBalance(
        [FromBody] CreateInventoryBalanceRequest request,
        CancellationToken cancellationToken)
    {
        var balance = await balanceAdminService.EnsureBalanceAsync(request, cancellationToken);
        return Ok(balance);
    }

    [HttpGet("balances/{id}/reservations")]
    [RequirePermission(PermissionCodes.InventoryRead)]
    public async Task<IActionResult> GetReservations(int id, CancellationToken cancellationToken)
    {
        var reservations = await queryService.GetReservationsByBalanceIdAsync(id, cancellationToken);
        return Ok(reservations);
    }

    [HttpGet("balances/{id}/movements")]
    [RequirePermission(PermissionCodes.InventoryRead)]
    public async Task<IActionResult> GetMovements(int id, CancellationToken cancellationToken)
    {
        var movements = await queryService.GetMovementsAsync(id, cancellationToken);
        return Ok(movements);
    }

    [HttpGet("balances/{id}/cost-layers")]
    [RequirePermission(PermissionCodes.InventoryRead)]
    public async Task<IActionResult> GetCostLayers(int id, CancellationToken cancellationToken)
    {
        var layers = await queryService.GetCostLayersAsync(id, cancellationToken);
        return Ok(layers);
    }

    [HttpPost("receive")]
    [RequirePermission(PermissionCodes.InventoryAdjust)]
    public async Task<IActionResult> Receive(
        [FromBody] ReceiveInventoryRequest request,
        CancellationToken cancellationToken)
    {
        var movement = await manualStockAdminService.ReceiveAsync(request, cancellationToken);
        return Ok(movement);
    }

    [HttpPost("issue")]
    [RequirePermission(PermissionCodes.InventoryAdjust)]
    public async Task<IActionResult> Issue(
        [FromBody] IssueInventoryRequest request,
        CancellationToken cancellationToken)
    {
        var movements = await manualStockAdminService.IssueAsync(request, cancellationToken);
        return Ok(movements);
    }

    [HttpPost("adjust")]
    [RequirePermission(PermissionCodes.InventoryAdjust)]
    public async Task<IActionResult> Adjust(
        [FromBody] AdjustInventoryRequest request,
        CancellationToken cancellationToken)
    {
        var movements = await manualStockAdminService.AdjustAsync(request, cancellationToken);
        return Ok(movements);
    }

    [HttpGet("reservations/{id}")]
    [RequirePermission(PermissionCodes.InventoryRead)]
    public async Task<IActionResult> GetReservation(int id, CancellationToken cancellationToken)
    {
        var reservation = await queryService.GetReservationByIdAsync(id, cancellationToken);
        return reservation is null ? NotFound() : Ok(reservation);
    }

    [HttpPost("reservations")]
    [RequirePermission(PermissionCodes.InventoryAdjust)]
    public async Task<IActionResult> Reserve(
        [FromBody] CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var reservations = await reservationAdminService.ReserveAsync(request, cancellationToken);
        return Ok(reservations);
    }

    [HttpPost("reservations/{id}/release")]
    [RequirePermission(PermissionCodes.InventoryAdjust)]
    public async Task<IActionResult> ReleaseReservation(int id, CancellationToken cancellationToken)
    {
        await reservationAdminService.ReleaseReservationAsync(id, cancellationToken);
        return NoContent();
    }
}
