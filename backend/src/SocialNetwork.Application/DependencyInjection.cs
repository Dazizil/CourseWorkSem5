using Microsoft.Extensions.DependencyInjection;
using SocialNetwork.Application.Auth;
using SocialNetwork.Application.Posts;
using SocialNetwork.Application.Roles;
using SocialNetwork.Application.Users;

namespace SocialNetwork.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPostService, PostService>();
        return services;
    }
}
