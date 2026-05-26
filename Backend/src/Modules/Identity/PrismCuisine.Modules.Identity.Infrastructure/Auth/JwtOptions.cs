namespace PrismCuisine.Modules.Identity.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "PrismCuisine";
    public string Audience { get; init; } = "PrismCuisine.Client";
    public string SigningKey { get; init; } = "CHANGE_ME_32_CHARACTERS_MINIMUM";
    public int AccessTokenMinutes { get; init; } = 30;
    public int RefreshTokenDays { get; init; } = 7;
}
