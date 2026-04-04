using Microsoft.EntityFrameworkCore;
using YashGems.Identity.Application.Interfaces;
using YashGems.Identity.Domain.Entities;
using YashGems.Identity.Domain.Enums;
using YashGems.Identity.Infrastructure.Data;

namespace YashGems.Identity.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;
    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
        => await _context.Users.ToListAsync();

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByIdAsync(Guid id)
        => await _context.Users.FindAsync(id);

    public async Task<IEnumerable<User>> GetUsersByKycStatusAsync(KycStatus status)
        => await _context.Users
            .Where(u => u.KycStatus == status)
            .ToListAsync();

    public async Task<bool> ExistsByEmailAsync(string email)
        => await _context.Users.AnyAsync(u => u.Email == email);

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<User>> GetPendingKycUsersAsync()
    {
        return await _context.Users
            .Where(u => u.KycStatus == KycStatus.Pending)
            .ToListAsync();
    }
}
