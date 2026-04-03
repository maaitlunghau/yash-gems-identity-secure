using YashGems.Identity.Domain.Entities;

namespace YashGems.Identity.Application.Interfaces;

public interface IOtpRepository
{
    Task AddAsync(OtpCode otp);

    Task<OtpCode?> GetLatestByEmailAsync(string email);

    Task UpdateAsync(OtpCode otp);
}
