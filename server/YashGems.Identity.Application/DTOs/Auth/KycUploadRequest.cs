using Microsoft.AspNetCore.Http;

namespace YashGems.Identity.Application.DTOs.Auth;

public class KycUploadRequest
{
    public IFormFile? IdCardFront { get; set; }
    public IFormFile? IdCardBack { get; set; }
    public IFormFile? FacePhoto { get; set; } = null!;
}
