using YashGems.Identity.Application.DTOs.Auth;

namespace YashGems.Identity.Application.Interfaces;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterRequest request);

    Task<AuthResponse?> LoginAsync(LoginRequest request);

    Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request);

    Task<bool> LogoutAsync(string refreshToken);
}
