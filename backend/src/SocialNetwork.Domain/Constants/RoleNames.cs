namespace SocialNetwork.Domain.Constants;

/// <summary>Системные роли. Имена используются в JWT-claims и атрибутах [Authorize(Roles = ...)].</summary>
public static class RoleNames
{
    public const string User = "User";
    public const string Moderator = "Moderator";
    public const string Admin = "Admin";

    public static readonly IReadOnlyList<string> All = [User, Moderator, Admin];
}
