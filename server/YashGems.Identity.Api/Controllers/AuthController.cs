using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using YashGems.Identity.Application.DTOs.Auth;
using YashGems.Identity.Application.Interfaces;

namespace YashGems.Identity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
            => _authService = authService;

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            if (!result) return BadRequest("Email đã tồn tại hoặc mật khẩu không hợp lệ.");

            return Ok("Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                if (response == null) return Unauthorized("Email hoặc mật khẩu không đúng.");

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request);
            if (response == null) return Unauthorized("Token không hợp lệ hoặc đã hết hạn.");

            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshTokenRequest request)
        {
            var result = await _authService.LogoutAsync(request.RefreshToken);
            if (!result) return BadRequest("Token không hợp lệ.");

            return Ok("Đăng xuất thành công.");
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            var result = await _authService.VerifyEmailAsync(request.Email, request.Code);
            if (!result) return BadRequest("Mã OTP không chính xác hoặc đã hết hạn.");

            return Ok("Xác thực Email thành công!");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var result = await _authService.ForgotPasswordAsync(request.Email);
            if (!result) return BadRequest("Email không tồn tại.");

            return Ok("Mã OTP đã được gửi");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request.Email, request.Otp, request.NewPassword);
            if (!result) return BadRequest("OTP không đúng hoặc đã hết hạn.");

            return Ok("Thay đổi mật khẩu thành công!");
        }

        [HttpPost("upload-kyc")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadKyc([FromForm] KycUploadRequest request)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var result = await _authService.UploadKycImagesAsync(email, request);
            if (!result) return BadRequest("Lỗi khi tải ảnh lên Cloudinary");

            return Ok("Tải ảnh eKYC thành công! Đang chờ duyệt.");
        }
        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var profile = await _authService.GetProfileAsync(email);
            if (profile == null) return NotFound("Không tìm thấy người dùng.");

            return Ok(profile);
        }

        [HttpPut("update-profile")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var result = await _authService.UpdateProfileAsync(email, request);
            if (!result) return BadRequest("Cập nhật thông tin thất bại.");

            return Ok("Cập nhật thông tin thành công!");
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            var response = await _authService.GoogleLoginAsync(request.IdToken);
            if (response == null)
            {
                return Unauthorized("Xác thực Google thất bại hoặc token không hợp lệ.");
            }

            return Ok(response);
        }

        [HttpPost("facebook-login")]
        public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginRequest request)
        {
            var response = await _authService.FacebookLoginAsync(request.AccessToken);
            if (response == null)
            {
                return Unauthorized("Xác thực Facebook thất bại hoặc tài khoản không cung cấp email.");
            }

            return Ok(response);
        }
    }
}
