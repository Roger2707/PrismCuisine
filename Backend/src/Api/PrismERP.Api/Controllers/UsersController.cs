using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Identity.Application.Authorization;
using PrismERP.Modules.Identity.Application.Users;
using PrismERP.Modules.Identity.Infrastructure.Auth.Authrizations;

namespace PrismERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/identity/users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("{id}")]
    [RequirePermission(PermissionCodes.UsersRead)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }
}
