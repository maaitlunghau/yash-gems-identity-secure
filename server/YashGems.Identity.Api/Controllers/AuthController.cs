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
    }
}
