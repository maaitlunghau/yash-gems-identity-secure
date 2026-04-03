using YashGems.Identity.Application.DTOs.Auth;
using YashGems.Identity.Application.DTOs.Messaging;
using YashGems.Identity.Application.Interfaces;
using YashGems.Identity.Application.Messaging;
using YashGems.Identity.Domain.Entities;

namespace YashGems.Identity.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _tokenRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly IMessageBusClient _messageBus;

    public AuthService(
       IUserRepository userRepository,
       IRefreshTokenRepository tokenRepository,
       ITokenProvider tokenProvider,
       IMessageBusClient messageBus)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _tokenProvider = tokenProvider;
        _messageBus = messageBus;
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var existingEmail = await _userRepository.ExistsByEmailAsync(request.Email);
        if (existingEmail) return false;

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber
        };

        await _userRepository.AddAsync(user);

        // send Message to RabbitMQ to send Email or SMS
        var otpMsg = new SendOtpMessage
        {
            Email = user.Email,
            OtpCode = new Random().Next(100000, 999999).ToString(),
            MessageType = "Email"
        };

        _messageBus.PublishNewMessage(otpMsg, "otp-routing-key");

        return true;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var (accessToken, jti) = _tokenProvider.CreateAccessToken(user);
        var refreshToken = _tokenProvider.CreateRefreshToken(user.Id, jti);

        await _tokenRepository.AddAsync(refreshToken);

        return new AuthResponse(
            accessToken,
            refreshToken.AccessToken,
            user.FullName,
            user.Email
        );
    }

    public async Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var storedToken = await _tokenRepository.GetByTokenAsync(request.RefreshToken);
        if (storedToken == null || !storedToken.IsActive) return null;

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user == null) return null;

        var (newAccessToken, jti) = _tokenProvider.CreateAccessToken(user);
        var newRefreshToken = _tokenProvider.CreateRefreshToken(user.Id, jti);

        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByRefreshToken = newRefreshToken.AccessToken;

        await _tokenRepository.UpdateAsync(storedToken);
        await _tokenRepository.AddAsync(newRefreshToken);

        return new AuthResponse(
            newAccessToken,
            newRefreshToken.AccessToken,
            user.FullName,
            user.Email
        );
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var storedToken = await _tokenRepository.GetByTokenAsync(refreshToken);
        if (storedToken == null || !storedToken.IsActive) return false;

        storedToken.RevokedAt = DateTime.UtcNow;

        await _tokenRepository.UpdateAsync(storedToken);
        return true;
    }
}
