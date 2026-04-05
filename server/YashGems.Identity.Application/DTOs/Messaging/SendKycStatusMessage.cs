namespace YashGems.Identity.Application.DTOs.Messaging;

public class SendKycStatusMessage
{
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
