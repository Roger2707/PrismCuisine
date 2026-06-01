using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrismCuisine.BuildingBlocks.Domain.Exceptions;
using PrismCuisine.Modules.Identity.Application.Auth;
using System.Security.Claims;

namespace PrismCuisine.Api.Controllers;

[ApiController]
[Route("api/identity/auth")]
public sealed class IdentityAuthController(IIdentityAuthService authService, IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        if (!string.IsNullOrEmpty(result.RefreshToken))
        {
            AppendRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        }
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

    [HttpPost("force-logout")]
    [Authorize(Roles = "super_admin")]
    public async Task<IActionResult> ForceLogout(int userId, CancellationToken cancellationToken)
    {
        await authService.ForceLogoutAsync(userId, cancellationToken);
        return Ok();
    }

    [HttpPost("release-blacklist")]
    [Authorize(Roles = "super_admin")]
    public async Task<IActionResult> ReleaseBlacklist(int userId, CancellationToken cancellationToken)
    {
        await authService.ReleaseBlacklistAsync(userId, cancellationToken);
        return Ok();
    }

    [HttpPost("refresh-page")]
    [Authorize(AuthenticationSchemes = "MyRefreshCookieScheme")]
    public async Task<IActionResult> RefreshPage(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized("Not found valid Refresh Token in Cookie.");
        }

        try
        {
            var result = await authService.RefreshPage(refreshToken, cancellationToken);
            AppendRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
            return Ok(new
            {
                accessToken = result.AccessToken,
                accessTokenExpiresAt = result.AccessTokenExpiresAt
            });
        }
        catch (DomainException ex) 
        {
            DeleteRefreshTokenCookie();
            return Unauthorized(ex.Message);
        }
    }

    private int GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!int.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid access token.");
        }

        return userId;
    }

    #region Helpers for Refresh Token Cookie

    [NonAction]
    private void AppendRefreshTokenCookie(string refreshToken, DateTime expiredAt)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = env.IsDevelopment() ? false : true,
            SameSite = SameSiteMode.Lax, 
            Expires = expiredAt
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    [NonAction]
    private void DeleteRefreshTokenCookie()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax
        };
        Response.Cookies.Delete("refreshToken", cookieOptions);
    }

    #endregion
}
