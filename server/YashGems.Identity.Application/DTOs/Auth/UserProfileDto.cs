namespace YashGems.Identity.Application.DTOs.Auth;

public class UserProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string KycStatus { get; set; } = string.Empty;
    public string? FacePhotoUrl { get; set; }
    public double? KycSimilarityScore { get; set; }
}
