using System.ComponentModel.DataAnnotations;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Application.Users;

public record UserDto(
    Guid Id,
    string Email,
    string UserName,
    string? DisplayName,
    string? Bio,
    string? AvatarUrl,
    bool IsBlocked,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt)
{
    public static UserDto From(User user, IReadOnlyList<string> roles) =>
        new(user.Id, user.Email, user.UserName, user.DisplayName, user.Bio, user.AvatarUrl,
            user.IsBlocked, roles, user.CreatedAt);
}

public record UpdateProfileRequest(
    [MaxLength(64)] string? DisplayName,
    [MaxLength(500)] string? Bio,
    [MaxLength(512), Url] string? AvatarUrl);
