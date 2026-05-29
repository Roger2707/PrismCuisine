using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismCuisine.Modules.Identity.Application.Permissions;

namespace PrismCuisine.Api.Controllers;

[ApiController]
[Route("api/identity/users")]
public sealed class PermissionController(IPermissionService permissionService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var permissions = await permissionService.GetPermissionsReadOnlyAsync(cancellationToken);
        return Ok(permissions);
    }

    [HttpGet("{id:string}")]
    [Authorize]
    public async Task<IActionResult> GetByCode(string id, CancellationToken cancellationToken)
    {
        var permission = await permissionService.GetPermissionReadOnlyByCodeAsync(id, cancellationToken);
        return permission is null ? NotFound() : Ok(permission);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreatePermissionRequest request, CancellationToken cancellationToken)
    {
        await permissionService.Add(request);
        return CreatedAtAction(nameof(GetByCode), new { id = request.Code }, request);
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update(UpdatePermissionRequest request, CancellationToken cancellationToken)
    {
        await permissionService.Update(request);
        return CreatedAtAction(nameof(GetByCode), new { id = request.Code }, request);
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var permission = await permissionService.GetPermissionByCodeAsync(id, cancellationToken);
        if (permission is null)
            return NotFound();
        await permissionService.Delete(id);
        return NoContent();
    }
}
