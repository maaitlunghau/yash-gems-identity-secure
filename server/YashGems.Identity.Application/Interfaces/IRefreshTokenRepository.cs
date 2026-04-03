using YashGems.Identity.Domain.Entities;

namespace YashGems.Identity.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);

    Task<RefreshToken?> GetByAccessTokenJtiAsync(string jti);

    Task<int> GetActiveCountByUserIdAsync(Guid userId);

    Task AddAsync(RefreshToken refreshToken);

    Task UpdateAsync(RefreshToken refreshToken);

    Task RevokeAllByUserIdAsync(Guid userId);

    Task DeleteExpiredAsync(DateTime cutoffDate);
}
