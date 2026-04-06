namespace YashGems.Identity.Application.DTOs.Auth;

public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
