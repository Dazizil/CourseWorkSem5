using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Posts;

public record PostDto(Guid Id, string Content, Guid AuthorId, string AuthorUserName, string? AuthorDisplayName, DateTime CreatedAt);

public record CreatePostRequest([Required, MinLength(1), MaxLength(5000)] string Content);
