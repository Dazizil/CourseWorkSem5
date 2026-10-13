using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Domain.Constants;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Application.Roles;

public class RoleService(IApplicationDbContext db, ICacheService cache) : IRoleService
{
    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct = default) =>
        await db.Roles.AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto(r.Id, r.Name, r.Description, r.UserRoles.Count))
            .ToListAsync(ct);

    public async Task AssignAsync(Guid userId, string roleName, Guid assignedById, CancellationToken ct = default)
    {
        var role = await FindRoleAsync(roleName, ct);
        if (!await db.Users.AnyAsync(u => u.Id == userId, ct))
            throw new NotFoundException("Пользователь не найден.");
        if (await db.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id, ct))
            throw new ConflictException($"У пользователя уже есть роль {role.Name}.");

        db.UserRoles.Add(new UserRole { UserId = userId, RoleId = role.Id, AssignedById = assignedById });
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync(CacheKeys.User(userId), ct);
    }

    public async Task RevokeAsync(Guid userId, string roleName, Guid revokedById, CancellationToken ct = default)
    {
        var role = await FindRoleAsync(roleName, ct);
        if (role.Name == RoleNames.User)
            throw new BadRequestException("Базовую роль User отозвать нельзя.");
        if (role.Name == RoleNames.Admin && userId == revokedById)
            throw new BadRequestException("Нельзя отозвать роль Admin у самого себя.");

        var userRole = await db.UserRoles.SingleOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id, ct)
                       ?? throw new NotFoundException($"У пользователя нет роли {role.Name}.");

        db.UserRoles.Remove(userRole);
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync(CacheKeys.User(userId), ct);
    }

    private async Task<Role> FindRoleAsync(string roleName, CancellationToken ct) =>
        await db.Roles.SingleOrDefaultAsync(r => r.Name == roleName, ct)
        ?? throw new NotFoundException($"Роль {roleName} не существует.");
}
