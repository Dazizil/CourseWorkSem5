using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Domain.Constants;
using SocialNetwork.Domain.Entities;
using SocialNetwork.Infrastructure.Identity;
using SocialNetwork.Infrastructure.Persistence;

namespace SocialNetwork.UnitTests.Common;

public static class TestFixture
{
    public static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new ApplicationDbContext(options);
        foreach (var name in RoleNames.All)
            db.Roles.Add(new Role { Name = name });
        db.SaveChanges();
        return db;
    }

    public static ITokenService CreateTokenService() =>
        new TokenService(Options.Create(new JwtOptions { Secret = new string('x', 64) }));
}

/// <summary>Кэш в памяти процесса — заменяет Redis в тестах.</summary>
public class FakeCacheService : ICacheService
{
    private readonly Dictionary<string, object?> _store = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) =>
        Task.FromResult(_store.TryGetValue(key, out var v) ? (T?)v : default);

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        _store[key] = value;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _store.Remove(key);
        return Task.CompletedTask;
    }
}
