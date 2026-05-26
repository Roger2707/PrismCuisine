using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismCuisine.Modules.Identity.Application.Auth;

namespace PrismCuisine.Api.Controllers;

[ApiController]
[Route("api/identity/auth")]
public sealed class IdentityAuthController(IIdentityAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await authService.LogoutAsync(userId, request, cancellationToken);
        return NoContent();
    }

    [HttpGet("current-user")]
    [Authorize]
    public async Task<IActionResult> CurrentUser(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var permissions = HttpContext.Items.TryGetValue("permissions", out var value) ? value ?? Array.Empty<string>() : Array.Empty<string>();
        var result = await authService.GetCurrentUserAsync(userId, permissions, cancellationToken);
        return Ok(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await authService.ChangePasswordAsync(userId, request, cancellationToken);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid access token.");
        }

        return userId;
    }
}
