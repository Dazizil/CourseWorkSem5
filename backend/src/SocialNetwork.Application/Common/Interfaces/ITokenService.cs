using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Application.Common.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateAccessToken(User user, IEnumerable<string> roles);

    /// <summary>Генерирует криптостойкий refresh-токен. Возвращает сам токен (для клиента) и его хэш (для БД).</summary>
    (string Token, string Hash, DateTime ExpiresAt) CreateRefreshToken();

    string HashRefreshToken(string token);
}
