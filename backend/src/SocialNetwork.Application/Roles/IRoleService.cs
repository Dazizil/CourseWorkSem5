namespace SocialNetwork.Application.Roles;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct = default);
    Task AssignAsync(Guid userId, string roleName, Guid assignedById, CancellationToken ct = default);
    Task RevokeAsync(Guid userId, string roleName, Guid revokedById, CancellationToken ct = default);
}
