using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Common.Models;
using SocialNetwork.Domain.Constants;

namespace SocialNetwork.Application.Users;

public class UserService(IApplicationDbContext db, ICacheService cache) : IUserService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var cached = await cache.GetAsync<UserDto>(CacheKeys.User(id), ct);
        if (cached is not null)
            return cached;

        var user = await db.Users.AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException("Пользователь не найден.");

        var dto = UserDto.From(user, user.UserRoles.Select(ur => ur.Role.Name).ToList());
        await cache.SetAsync(CacheKeys.User(id), dto, CacheTtl, ct);
        return dto;
    }

    public async Task<PagedResult<UserDto>> SearchAsync(string? query, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var users = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim().ToLower();
            users = users.Where(u => u.UserName.Contains(q) ||
                                     (u.DisplayName != null && u.DisplayName.ToLower().Contains(q)));
        }

        var total = await users.CountAsync(ct);
        var items = await users
            .OrderBy(u => u.UserName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new UserDto(u.Id, u.Email, u.UserName, u.DisplayName, u.Bio, u.AvatarUrl, u.IsBlocked,
                u.UserRoles.Select(ur => ur.Role.Name).ToList(), u.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<UserDto>(items, page, pageSize, total);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == id, ct)
                   ?? throw new NotFoundException("Пользователь не найден.");

        user.DisplayName = request.DisplayName?.Trim();
        user.Bio = request.Bio?.Trim();
        user.AvatarUrl = request.AvatarUrl?.Trim();
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await cache.RemoveAsync(CacheKeys.User(id), ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task SetBlockedAsync(Guid id, bool isBlocked, CancellationToken ct = default)
    {
        var user = await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException("Пользователь не найден.");

        if (user.UserRoles.Any(ur => ur.Role.Name == RoleNames.Admin))
            throw new ForbiddenException("Нельзя заблокировать администратора.");

        user.IsBlocked = isBlocked;
        user.UpdatedAt = DateTime.UtcNow;

        if (isBlocked)
        {
            // Блокировка отзывает все активные refresh-токены — пользователь не сможет продлить сессию.
            await db.RefreshTokens
                .Where(t => t.UserId == id && t.RevokedAt == null)
                .ForEachAsync(t => t.RevokedAt = DateTime.UtcNow, ct);
        }

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync(CacheKeys.User(id), ct);
    }
}
