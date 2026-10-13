using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Roles;
using SocialNetwork.Domain.Constants;

namespace SocialNetwork.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
[Route("api/roles")]
public class RolesController(IRoleService roleService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<RoleDto>> GetAll(CancellationToken ct) => roleService.GetAllAsync(ct);

    [HttpPost("users/{userId:guid}")]
    public async Task<IActionResult> Assign(Guid userId, AssignRoleRequest request, CancellationToken ct)
    {
        await roleService.AssignAsync(userId, request.RoleName, currentUser.UserId!.Value, ct);
        return NoContent();
    }

    [HttpDelete("users/{userId:guid}/{roleName}")]
    public async Task<IActionResult> Revoke(Guid userId, string roleName, CancellationToken ct)
    {
        await roleService.RevokeAsync(userId, roleName, currentUser.UserId!.Value, ct);
        return NoContent();
    }
}
