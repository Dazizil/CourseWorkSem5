namespace SocialNetwork.Infrastructure.Identity;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "SocialNetwork";
    public string Audience { get; set; } = "SocialNetwork";
    /// <summary>Симметричный ключ HMAC-SHA256, минимум 32 символа.</summary>
    public string Secret { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
