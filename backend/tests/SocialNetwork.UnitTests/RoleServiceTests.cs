using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Roles;
using SocialNetwork.Domain.Constants;
using SocialNetwork.Domain.Entities;
using SocialNetwork.UnitTests.Common;

namespace SocialNetwork.UnitTests;

public class RoleServiceTests
{
    [Fact]
    public async Task Assign_ThenRevoke_Moderator()
    {
        var db = TestFixture.CreateDb();
        var user = new User { Email = "u@u.ru", UserName = "u", PasswordHash = "x" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var service = new RoleService(db, new FakeCacheService());
        var adminId = Guid.NewGuid();

        await service.AssignAsync(user.Id, RoleNames.Moderator, adminId);
        Assert.True(await db.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.Role.Name == RoleNames.Moderator));

        await service.RevokeAsync(user.Id, RoleNames.Moderator, adminId);
        Assert.False(await db.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.Role.Name == RoleNames.Moderator));
    }

    [Fact]
    public async Task Revoke_BaseUserRole_Throws()
    {
        var service = new RoleService(TestFixture.CreateDb(), new FakeCacheService());

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.RevokeAsync(Guid.NewGuid(), RoleNames.User, Guid.NewGuid()));
    }

    [Fact]
    public async Task Revoke_OwnAdminRole_Throws()
    {
        var service = new RoleService(TestFixture.CreateDb(), new FakeCacheService());
        var adminId = Guid.NewGuid();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.RevokeAsync(adminId, RoleNames.Admin, adminId));
    }
}
