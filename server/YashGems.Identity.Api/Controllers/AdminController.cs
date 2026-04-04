using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YashGems.Identity.Application.Interfaces;

namespace YashGems.Identity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AdminController(IAuthService authService)
            => _authService = authService;

        [HttpGet("pending-kyc")]
        public async Task<IActionResult> GetPendingKyc()
        {
            var users = await _authService.GetPendingKycUsersAsync();
            return Ok(users);
        }

        [HttpPost("approve-kyc/{userId}")]
        public async Task<IActionResult> ApproveKyc(Guid userId)
        {
            var result = await _authService.ApproveKycAsync(userId);
            if (!result) return BadRequest("Lỗi khi duyệt eKYC");

            return Ok("Duyệt thành công");
        }

        [HttpPost("reject-kyc/{userId}")]
        public async Task<IActionResult> RejectKyc(Guid userId)
        {
            var result = await _authService.RejectKycAsync(userId);
            if (!result) return BadRequest("Lỗi khi từ chối eKYC");

            return Ok("Đã từ chối eKYC và xoá ảnh cũ");
        }
    }
}
