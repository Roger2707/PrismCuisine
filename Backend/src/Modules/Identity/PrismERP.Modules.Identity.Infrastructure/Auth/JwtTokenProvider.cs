using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PrismERP.Modules.Identity.Application.Abstractions.Services;

namespace PrismERP.Modules.Identity.Infrastructure.Auth;

internal sealed class JwtTokenProvider(IOptions<JwtOptions> options) : IJwtTokenProvider
{
    private readonly JwtOptions _jwt = options.Value;

    public (string token, DateTime expiresAt) CreateAccessToken(int userId, string email, IReadOnlyCollection<string> roles)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public (string token, DateTime expiresAt) CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(48);
        var token = Convert.ToBase64String(bytes);
        var expiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);
        return (token, expiresAt);
    }
}
