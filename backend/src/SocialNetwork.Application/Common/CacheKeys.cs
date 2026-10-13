namespace SocialNetwork.Application.Common;

public static class CacheKeys
{
    public static string User(Guid id) => $"user:{id}";
}
