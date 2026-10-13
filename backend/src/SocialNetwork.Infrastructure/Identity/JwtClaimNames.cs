namespace SocialNetwork.Infrastructure.Identity;

/// <summary>
/// Короткие имена claims в JWT. Api отключает MapInboundClaims и использует эти же имена
/// как NameClaimType/RoleClaimType, поэтому [Authorize(Roles = ...)] работает напрямую.
/// </summary>
public static class JwtClaimNames
{
    public const string Role = "role";
    public const string Subject = "sub";
}
