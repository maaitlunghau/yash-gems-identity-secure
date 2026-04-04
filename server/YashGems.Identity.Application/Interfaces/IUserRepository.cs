using YashGems.Identity.Domain.Entities;
using YashGems.Identity.Domain.Enums;

namespace YashGems.Identity.Application.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();

    Task<User?> GetByIdAsync(Guid id);

    Task<User?> GetByEmailAsync(string email);

    Task<IEnumerable<User>> GetUsersByKycStatusAsync(KycStatus status);

    Task<IEnumerable<User>> GetPendingKycUsersAsync();

    Task<bool> ExistsByEmailAsync(string email);

    Task AddAsync(User user);

    Task UpdateAsync(User user);

    Task DeleteAsync(User user);
}
