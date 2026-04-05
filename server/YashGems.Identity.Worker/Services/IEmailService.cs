namespace YashGems.Identity.Worker.Services;

public interface IEmailService
{
    Task SendOtpEmailAsync(string toEmail, string otpCode, string messageType);
    Task SendKycStatusEmailAsync(string toEmail, string status);
}
