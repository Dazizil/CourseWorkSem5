using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Domain.Constants;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Infrastructure.Persistence;

/// <summary>Применяет миграции и создаёт системные роли и первого администратора (из секции SeedAdmin).</summary>
public static class DbInitializer
{
    private static readonly Dictionary<string, string> RoleDescriptions = new()
    {
        [RoleNames.User] = "Обычный пользователь",
        [RoleNames.Moderator] = "Модерация контента",
        [RoleNames.Admin] = "Полный доступ, управление ролями",
    };

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));

        await db.Database.MigrateAsync(ct);

        foreach (var name in RoleNames.All)
        {
            if (!await db.Roles.AnyAsync(r => r.Name == name, ct))
                db.Roles.Add(new Role { Name = name, Description = RoleDescriptions[name] });
        }
        await db.SaveChangesAsync(ct);

        var email = config["SeedAdmin:Email"];
        var password = config["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        email = email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email, ct))
            return;

        var roles = await db.Roles.ToListAsync(ct);
        var admin = new User
        {
            Email = email,
            UserName = config["SeedAdmin:UserName"]?.Trim().ToLowerInvariant() ?? "admin",
            DisplayName = "Administrator",
            PasswordHash = hasher.Hash(password),
        };
        foreach (var role in roles)
            admin.UserRoles.Add(new UserRole { User = admin, Role = role });

        db.Users.Add(admin);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Создан администратор {Email}", email);
    }
}
