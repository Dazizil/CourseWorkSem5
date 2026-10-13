using SocialNetwork.Domain.Common;

namespace SocialNetwork.Domain.Entities;

public class Post : BaseEntity
{
    public required string Content { get; set; }

    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
}
