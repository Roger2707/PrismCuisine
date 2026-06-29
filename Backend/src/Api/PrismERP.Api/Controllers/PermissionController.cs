using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Identity.Application.Authorization;
using PrismERP.Modules.Identity.Application.Permissions;
using PrismERP.Modules.Identity.Infrastructure.Auth.Authrizations;

namespace PrismERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/identity/permissions")]
public sealed class PermissionController(IPermissionService permissionService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(PermissionCodes.RolesRead)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var permissions = await permissionService.GetPermissionsReadOnlyAsync(cancellationToken);
        return Ok(permissions);
    }

    [HttpGet("{id}")]
    [RequirePermission(PermissionCodes.RolesRead)]
    public async Task<IActionResult> GetByCode(string id, CancellationToken cancellationToken)
    {
        var permission = await permissionService.GetPermissionReadOnlyByCodeAsync(id, cancellationToken);
        return permission is null ? NotFound() : Ok(permission);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.RolesWrite)]
    public async Task<IActionResult> Create(CreatePermissionRequest request, CancellationToken cancellationToken)
    {
        await permissionService.Add(request);
        return CreatedAtAction(nameof(GetByCode), new { id = request.Code }, request);
    }

    [HttpPut]
    [RequirePermission(PermissionCodes.RolesWrite)]
    public async Task<IActionResult> Update(UpdatePermissionRequest request, CancellationToken cancellationToken)
    {
        await permissionService.Update(request);
        return CreatedAtAction(nameof(GetByCode), new { id = request.Code }, request);
    }

    [HttpDelete]
    [RequirePermission(PermissionCodes.RolesWrite)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var permission = await permissionService.GetPermissionByCodeAsync(id, cancellationToken);
        if (permission is null)
            return NotFound();
        await permissionService.Delete(id);
        return NoContent();
    }
}
