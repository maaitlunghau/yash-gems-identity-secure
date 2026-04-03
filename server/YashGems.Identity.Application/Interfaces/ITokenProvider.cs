using YashGems.Identity.Domain.Entities;

namespace YashGems.Identity.Application.Interfaces;

public interface ITokenProvider
{
    (string AccessToken, string Jti) CreateAccessToken(User user);

    RefreshToken CreateRefreshToken(Guid userId, string accessTokenJti);

    string GenerateRefreshTokenString();
}
