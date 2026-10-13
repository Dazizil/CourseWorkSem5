using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Common.Models;
using SocialNetwork.Application.Users;
using SocialNetwork.Domain.Constants;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController(IUserService userService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("me")]
    public Task<UserDto> Me(CancellationToken ct) => userService.GetByIdAsync(currentUser.UserId!.Value, ct);

    [HttpPut("me")]
    public Task<UserDto> UpdateMe(UpdateProfileRequest request, CancellationToken ct) =>
        userService.UpdateProfileAsync(currentUser.UserId!.Value, request, ct);

    [HttpGet("{id:guid}")]
    public Task<UserDto> Get(Guid id, CancellationToken ct) => userService.GetByIdAsync(id, ct);

    [HttpGet]
    public Task<PagedResult<UserDto>> Search([FromQuery] string? query, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        userService.SearchAsync(query, page, pageSize, ct);

    [Authorize(Roles = $"{RoleNames.Moderator},{RoleNames.Admin}")]
    [HttpPost("{id:guid}/block")]
    public async Task<IActionResult> Block(Guid id, CancellationToken ct)
    {
        await userService.SetBlockedAsync(id, true, ct);
        return NoContent();
    }

    [Authorize(Roles = $"{RoleNames.Moderator},{RoleNames.Admin}")]
    [HttpPost("{id:guid}/unblock")]
    public async Task<IActionResult> Unblock(Guid id, CancellationToken ct)
    {
        await userService.SetBlockedAsync(id, false, ct);
        return NoContent();
    }
}
