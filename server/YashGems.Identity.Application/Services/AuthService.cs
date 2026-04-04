using CloudinaryDotNet.Actions;
using YashGems.Identity.Application.DTOs.Auth;
using YashGems.Identity.Application.DTOs.Messaging;
using YashGems.Identity.Application.Interfaces;
using YashGems.Identity.Application.Messaging;
using YashGems.Identity.Domain.Entities;
using YashGems.Identity.Domain.Enums;

namespace YashGems.Identity.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _tokenRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly IMessageBusClient _messageBus;
    private readonly IOtpRepository _otpRepository;
    private readonly IPhotoService _photoService;

    public AuthService(
       IUserRepository userRepository,
       IRefreshTokenRepository tokenRepository,
       ITokenProvider tokenProvider,
       IMessageBusClient messageBus,
       IOtpRepository otpRepository,
       IPhotoService photoService
    )
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _tokenProvider = tokenProvider;
        _messageBus = messageBus;
        _otpRepository = otpRepository;
        _photoService = photoService;
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

        var otpCode = new Random().Next(100000, 999999).ToString();
        var otpRecord = new OtpCode
        {
            Email = user.Email,
            Code = otpCode,
            ExpiryDate = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _otpRepository.AddAsync(otpRecord);

        _messageBus.PublishNewMessage(new SendOtpMessage
        {
            Email = request.Email,
            OtpCode = otpCode,
            MessageType = "Email"
        }, "otp-routing-key");

        return true;
    }

    public async Task<bool> VerifyEmailAsync(string email, string code)
    {
        var otpRecord = await _otpRepository.GetLatestByEmailAsync(email);
        if (otpRecord is null || otpRecord.Code != code || otpRecord.ExpiryDate < DateTime.UtcNow)
        {
            return false;
        }

        var user = await _userRepository.GetByEmailAsync(email);
        if (user is not null)
        {
            otpRecord.IsUsed = true;
            await _otpRepository.UpdateAsync(otpRecord);

            user.Status = UserStatus.Verified;
            await _userRepository.UpdateAsync(user);

            return true;
        }

        return false;
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

    public async Task<bool> UploadKycImagesAsync(string email, KycUploadRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null) return false;

        if (request.IdCardFront == null || request.IdCardBack == null)
        {
            Console.WriteLine("Upload thiếu ảnh mặt trước hoặc ảnh mặt sau!");
            return false;
        }

        var oldFrontId = user.IdCardFrontPublicId;
        var oldBackId = user.IdCardBackPublicId;

        // upload mặt trước
        var frontResult = await _photoService.AddPhotoAsync(request.IdCardFront);
        if (frontResult.Error != null || frontResult.SecureUrl == null) return false;

        // upload mặt sau
        var backResult = await _photoService.AddPhotoAsync(request.IdCardBack);
        if (backResult.Error != null || backResult.SecureUrl == null) return false;

        user.IdCardFrontUrl = frontResult.SecureUrl.AbsoluteUri;
        user.IdCardBackUrl = backResult.SecureUrl.AbsoluteUri;

        user.IdCardFrontPublicId = frontResult.PublicId;
        user.IdCardBackPublicId = backResult.PublicId;

        user.KycStatus = KycStatus.Pending;
        await _userRepository.UpdateAsync(user);

        if (!string.IsNullOrEmpty(oldFrontId))
            await _photoService.DeletionResultAsync(oldFrontId);

        if (!string.IsNullOrEmpty(oldBackId))
            await _photoService.DeletionResultAsync(oldBackId);

        return true;
    }
}
