using SocialNetwork.Application.Auth;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Domain.Constants;
using SocialNetwork.Infrastructure.Identity;
using SocialNetwork.UnitTests.Common;

namespace SocialNetwork.UnitTests;

public class AuthServiceTests
{
    private static AuthService CreateService(out Infrastructure.Persistence.ApplicationDbContext db)
    {
        db = TestFixture.CreateDb();
        return new AuthService(db, new BCryptPasswordHasher(), TestFixture.CreateTokenService());
    }

    [Fact]
    public async Task Register_AssignsUserRole_AndReturnsTokens()
    {
        var service = CreateService(out _);

        var result = await service.RegisterAsync(new RegisterRequest("Ivan@Mail.ru", "ivan", "Password123", "Иван"));

        Assert.Equal("ivan@mail.ru", result.User.Email);
        Assert.Equal([RoleNames.User], result.User.Roles);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));
    }

    [Fact]
    public async Task Register_DuplicateEmail_Throws()
    {
        var service = CreateService(out _);
        await service.RegisterAsync(new RegisterRequest("a@a.ru", "user1", "Password123", null));

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.RegisterAsync(new RegisterRequest("A@A.ru", "user2", "Password123", null)));
    }

    [Fact]
    public async Task Login_WrongPassword_Throws()
    {
        var service = CreateService(out _);
        await service.RegisterAsync(new RegisterRequest("a@a.ru", "user1", "Password123", null));

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.LoginAsync(new LoginRequest("user1", "wrong-password")));
    }

    [Fact]
    public async Task Refresh_RotatesToken_OldTokenBecomesInvalid()
    {
        var service = CreateService(out _);
        var registered = await service.RegisterAsync(new RegisterRequest("a@a.ru", "user1", "Password123", null));

        var refreshed = await service.RefreshAsync(registered.RefreshToken);

        Assert.NotEqual(registered.RefreshToken, refreshed.RefreshToken);
        await Assert.ThrowsAsync<UnauthorizedException>(() => service.RefreshAsync(registered.RefreshToken));
    }
}
