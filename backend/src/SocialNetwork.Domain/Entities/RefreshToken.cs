using SocialNetwork.Domain.Common;

namespace SocialNetwork.Domain.Entities;

public class RefreshToken : BaseEntity
{
    /// <summary>SHA-256 хэш токена; сам токен в БД не хранится.</summary>
    public required string TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
}
