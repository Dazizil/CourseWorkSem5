using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Application.Auth;

public record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(3), MaxLength(32), RegularExpression("^[a-zA-Z0-9_.]+$")] string UserName,
    [Required, MinLength(8), MaxLength(128)] string Password,
    [MaxLength(64)] string? DisplayName);

public record LoginRequest(
    [Required] string Login,
    [Required] string Password);

public record RefreshRequest([Required] string RefreshToken);

public record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    Users.UserDto User);
