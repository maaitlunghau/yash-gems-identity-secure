using YashGems.Identity.Domain.Entities;

namespace YashGems.Identity.Application.Interfaces;

public interface IOtpRepository
{
    Task AddAsync(OtpCode otp);

    Task<OtpCode?> GetLatestByEmailAsync(string email);

    Task<bool> ValidateOtpAsync(string email, string code);

    Task UpdateAsync(OtpCode otp);
}
