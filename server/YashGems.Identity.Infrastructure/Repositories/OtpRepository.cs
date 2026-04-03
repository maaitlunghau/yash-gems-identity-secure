using Microsoft.EntityFrameworkCore;
using YashGems.Identity.Application.Interfaces;
using YashGems.Identity.Domain.Entities;
using YashGems.Identity.Infrastructure.Data;

namespace YashGems.Identity.Infrastructure.Repositories;

public class OtpRepository : IOtpRepository
{
    private readonly IdentityDbContext _context;

    public OtpRepository(IdentityDbContext context) => _context = context;

    public async Task AddAsync(OtpCode otp)
    {
        await _context.OtpCodes.AddAsync(otp);
        await _context.SaveChangesAsync();
    }

    public async Task<OtpCode?> GetLatestByEmailAsync(string email)
        => await _context.OtpCodes
            .Where(o => o.Email == email && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task UpdateAsync(OtpCode otp)
    {
        _context.OtpCodes.Update(otp);
        await _context.SaveChangesAsync();
    }
}
