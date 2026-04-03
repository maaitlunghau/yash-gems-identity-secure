using Microsoft.EntityFrameworkCore;
using YashGems.Identity.Application.Interfaces;
using YashGems.Identity.Domain.Entities;
using YashGems.Identity.Infrastructure.Data;

namespace YashGems.Identity.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IdentityDbContext _context;
    public RefreshTokenRepository(IdentityDbContext context)
    {
        _context = context;
    }


    public async Task<int> GetActiveCountByUserIdAsync(Guid userId)
        => await _context.RefreshTokens.CountAsync(rt =>
            rt.UserId == userId &&
            rt.RevokedAt == null &&
            rt.ExpiryDate > DateTime.UtcNow
        );

    public async Task<RefreshToken?> GetByAccessTokenJtiAsync(string jti)
        => await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.AccessTokenJti == jti);

    public async Task<RefreshToken?> GetByTokenAsync(string token)
        => await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.AccessToken == token);

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllByUserIdAsync(Guid userId)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt =>
                rt.UserId == userId &&
                rt.RevokedAt == null &&
                rt.ExpiryDate > DateTime.UtcNow
            )
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }


    public async Task DeleteExpiredAsync(DateTime cutoffDate)
    {
        var expiredTokens = await _context.RefreshTokens
        .Where(rt => rt.ExpiryDate <= cutoffDate)
        .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
    }
}
