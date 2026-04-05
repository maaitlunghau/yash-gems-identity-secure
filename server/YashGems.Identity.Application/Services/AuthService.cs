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
    private readonly IAiFaceService _aiFaceService;

    public AuthService(
       IUserRepository userRepository,
       IRefreshTokenRepository tokenRepository,
       ITokenProvider tokenProvider,
       IMessageBusClient messageBus,
       IOtpRepository otpRepository,
       IPhotoService photoService,
        IAiFaceService aiFaceService
    )
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _tokenProvider = tokenProvider;
        _messageBus = messageBus;
        _otpRepository = otpRepository;
        _photoService = photoService;
        _aiFaceService = aiFaceService;
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

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null) return false;

        var otpCode = new Random().Next(100000, 999999).ToString();
        var otpEntry = new OtpCode
        {
            Email = email,
            Code = otpCode,
            ExpiryDate = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _otpRepository.AddAsync(otpEntry);

        var otpMessage = new SendOtpMessage
        {
            Email = email,
            OtpCode = otpCode,
            MessageType = "ForgotPassword"
        };

        _messageBus.PublishNewMessage(otpMessage, "otp-routing-key");

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email, string otp, string newPassword)
    {
        var isValidOtp = await _otpRepository.ValidateOtpAsync(email, otp);
        if (!isValidOtp) return false;

        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);

        return true;
    }

    public async Task<bool> UploadKycImagesAsync(string email, KycUploadRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null) return false;

        if (request.IdCardFront == null || request.IdCardBack == null || request.FacePhoto == null)
        {
            Console.WriteLine("--> LỖI: Thiếu ảnh mặt trước, mặt sau hoặc ảnh chân dung.");
            return false;
        }

        var oldFrontId = user.IdCardFrontPublicId;
        var oldBackId = user.IdCardBackPublicId;
        var oldFaceId = user.FacePhotoPublicId;

        var frontResult = await _photoService.AddPhotoAsync(request.IdCardFront);
        var backResult = await _photoService.AddPhotoAsync(request.IdCardBack);
        var faceResult = await _photoService.AddPhotoAsync(request.FacePhoto);

        if (frontResult.Error != null || backResult.Error != null || faceResult.Error != null)
        {
            return false;
        }

        user.IdCardFrontUrl = frontResult.SecureUrl.AbsoluteUri;
        user.IdCardBackUrl = backResult.SecureUrl.AbsoluteUri;
        user.FacePhotoUrl = faceResult.SecureUrl.AbsoluteUri;

        user.IdCardFrontPublicId = frontResult.PublicId;
        user.IdCardBackPublicId = backResult.PublicId;
        user.FacePhotoPublicId = faceResult.PublicId;

        var similarity = await _aiFaceService.CompareFacesAsync(user.FacePhotoUrl, user.IdCardFrontUrl);
        user.KycSimilarityScore = similarity;

        if (similarity >= 80.0)
        {
            user.KycStatus = KycStatus.Verified;
        }
        else if (similarity >= 60.0)
        {
            user.KycStatus = KycStatus.Pending;
        }
        else
        {
            user.KycStatus = KycStatus.Rejected;
        }

        await _userRepository.UpdateAsync(user);

        if (!string.IsNullOrEmpty(oldFrontId))
            await _photoService.DeletionResultAsync(oldFrontId);
        if (!string.IsNullOrEmpty(oldBackId))
            await _photoService.DeletionResultAsync(oldBackId);
        if (!string.IsNullOrEmpty(oldFaceId))
            await _photoService.DeletionResultAsync(oldFaceId);

        return true;
    }

    public async Task<IEnumerable<User>> GetPendingKycUsersAsync()
    {
        return await _userRepository.GetPendingKycUsersAsync();
    }

    public async Task<bool> ApproveKycAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) return false;

        user.KycStatus = KycStatus.Verified;
        await _userRepository.UpdateAsync(user);

        return true;
    }

    public async Task<bool> RejectKycAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) return false;

        user.KycStatus = KycStatus.Rejected;

        // xoá eKYC đã upload (2 mặt trước và sau)
        if (!string.IsNullOrEmpty(user.IdCardFrontPublicId))
            await _photoService.DeletionResultAsync(user.IdCardFrontPublicId);

        if (!string.IsNullOrEmpty(user.IdCardBackPublicId))
            await _photoService.DeletionResultAsync(user.IdCardBackPublicId);

        user.IdCardFrontUrl = null;
        user.IdCardFrontPublicId = null;
        user.IdCardBackUrl = null;
        user.IdCardBackPublicId = null;

        await _userRepository.UpdateAsync(user);
        return true;
    }
}
