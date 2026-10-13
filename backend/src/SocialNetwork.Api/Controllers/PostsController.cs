using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Models;
using SocialNetwork.Application.Posts;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/posts")]
public class PostsController(IPostService postService) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<PostDto>> Feed([FromQuery] Guid? authorId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        postService.GetFeedAsync(authorId, page, pageSize, ct);

    [HttpPost]
    public Task<PostDto> Create(CreatePostRequest request, CancellationToken ct) => postService.CreateAsync(request, ct);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await postService.DeleteAsync(id, ct);
        return NoContent();
    }
}
