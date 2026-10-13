using SocialNetwork.Application.Common.Models;

namespace SocialNetwork.Application.Posts;

public interface IPostService
{
    Task<PagedResult<PostDto>> GetFeedAsync(Guid? authorId, int page, int pageSize, CancellationToken ct = default);
    Task<PostDto> CreateAsync(CreatePostRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
