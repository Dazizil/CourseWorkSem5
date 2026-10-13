using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Users;
using SocialNetwork.Domain.Constants;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Application.Auth;

public class AuthService(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var userName = request.UserName.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == email, ct))
            throw new ConflictException("Пользователь с таким email уже существует.");
        if (await db.Users.AnyAsync(u => u.UserName == userName, ct))
            throw new ConflictException("Имя пользователя уже занято.");

        var userRole = await db.Roles.SingleAsync(r => r.Name == RoleNames.User, ct);

        var user = new User
        {
            Email = email,
            UserName = userName,
            DisplayName = request.DisplayName?.Trim(),
            PasswordHash = passwordHasher.Hash(request.Password),
        };
        user.UserRoles.Add(new UserRole { User = user, Role = userRole });
        db.Users.Add(user);

        return await IssueTokensAsync(user, [RoleNames.User], ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var login = request.Login.Trim().ToLowerInvariant();
        var user = await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(u => u.Email == login || u.UserName == login, ct);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Неверный логин или пароль.");
        if (user.IsBlocked)
            throw new ForbiddenException("Учётная запись заблокирована.");

        return await IssueTokensAsync(user, user.UserRoles.Select(ur => ur.Role.Name).ToList(), ct);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = tokenService.HashRefreshToken(refreshToken);
        var stored = await db.RefreshTokens
            .Include(t => t.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (stored is null || !stored.IsActive)
            throw new UnauthorizedException("Refresh-токен недействителен.");
        if (stored.User.IsBlocked)
            throw new ForbiddenException("Учётная запись заблокирована.");

        // Ротация: старый токен отзывается, выдаётся новая пара.
        stored.RevokedAt = DateTime.UtcNow;
        var roles = stored.User.UserRoles.Select(ur => ur.Role.Name).ToList();
        return await IssueTokensAsync(stored.User, roles, ct);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = tokenService.HashRefreshToken(refreshToken);
        var stored = await db.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (stored is { RevokedAt: null })
        {
            stored.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, IReadOnlyList<string> roles, CancellationToken ct)
    {
        var (accessToken, accessExpires) = tokenService.CreateAccessToken(user, roles);
        var (refreshToken, refreshHash, refreshExpires) = tokenService.CreateRefreshToken();

        db.RefreshTokens.Add(new RefreshToken { TokenHash = refreshHash, ExpiresAt = refreshExpires, User = user });
        await db.SaveChangesAsync(ct);

        return new AuthResponse(accessToken, accessExpires, refreshToken, refreshExpires, UserDto.From(user, roles));
    }
}
