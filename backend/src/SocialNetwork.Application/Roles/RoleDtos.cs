using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Roles;

public record RoleDto(Guid Id, string Name, string? Description, int UsersCount);

public record AssignRoleRequest([Required] string RoleName);
