using YashGems.Identity.Application.DTOs.Auth;
using YashGems.Identity.Domain.Entities;

namespace YashGems.Identity.Application.Interfaces;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterRequest request);

    Task<AuthResponse?> LoginAsync(LoginRequest request);

    Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request);

    Task<bool> LogoutAsync(string refreshToken);

    Task<bool> VerifyEmailAsync(string email, string code);

    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string otp, string newPassword);

    Task<bool> UploadKycImagesAsync(string email, KycUploadRequest request);

    Task<IEnumerable<User>> GetPendingKycUsersAsync();

    Task<bool> ApproveKycAsync(Guid userId);

    Task<bool> RejectKycAsync(Guid userId);

    Task<UserProfileDto?> GetProfileAsync(string email);
}
