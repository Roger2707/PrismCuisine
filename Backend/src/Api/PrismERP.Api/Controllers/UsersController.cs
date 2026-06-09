using Microsoft.AspNetCore.Mvc;
using PrismERP.Modules.Identity.Application.Users;

namespace PrismERP.Api.Controllers;

[ApiController]
[Route("api/identity/users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }
}
