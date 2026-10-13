using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Infrastructure.Identity;

namespace SocialNetwork.Api.Services;

public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public Guid? UserId =>
        Guid.TryParse(accessor.HttpContext?.User.FindFirst(JwtClaimNames.Subject)?.Value, out var id) ? id : null;

    public bool IsInRole(string role) => accessor.HttpContext?.User.IsInRole(role) ?? false;
}
