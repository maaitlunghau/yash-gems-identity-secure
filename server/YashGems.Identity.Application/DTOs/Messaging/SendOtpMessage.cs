namespace YashGems.Identity.Application.DTOs.Messaging;

public class SendOtpMessage
{
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public string MessageType { get; set; } = "Email"; // Hoặc "SMS"
}
