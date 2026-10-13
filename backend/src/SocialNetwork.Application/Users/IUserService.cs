using SocialNetwork.Application.Common.Models;

namespace SocialNetwork.Application.Users;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<UserDto>> SearchAsync(string? query, int page, int pageSize, CancellationToken ct = default);
    Task<UserDto> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken ct = default);
    Task SetBlockedAsync(Guid id, bool isBlocked, CancellationToken ct = default);
}
