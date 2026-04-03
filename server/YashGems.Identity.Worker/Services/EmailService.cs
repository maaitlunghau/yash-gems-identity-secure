using MailKit.Net.Smtp;
using MimeKit;

namespace YashGems.Identity.Worker.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
        => _configuration = configuration;

    public async Task SendOtpEmailAsync(string toEmail, string otpCode)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");

        var message = new MimeMessage();

        var senderName = emailSettings["SenderName"]
            ?? throw new ArgumentNullException();
        var senderEmail = emailSettings["SenderEmail"]
            ?? throw new ArgumentNullException();
        var host = emailSettings["Host"]
            ?? throw new ArgumentNullException();
        var port = int.Parse(emailSettings["Port"]
            ?? throw new ArgumentNullException());
        var appPassword = emailSettings["AppPassword"]
            ?? throw new ArgumentNullException();

        message.From.Add(new MailboxAddress(senderEmail, senderEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = "Mã xác thực OTP của bạn - Yash Gems Identity Secure";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                <h2 style='color: #d4af37; text-align: center;'>YASH GEMS IDENTITY</h2>
                <p>Chào bạn,</p>
                <p>Bạn vừa yêu cầu mã xác thực để đăng nhập hoặc đăng ký tại hệ thống của chúng tôi.</p>
                <div style='background-color: #f9f9f9; padding: 15px; text-align: center; border-radius: 5px;'>
                    <span style='font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #333;'>{otpCode}</span>
                </div>
                <p style='color: #888; font-size: 12px; margin-top: 20px;'>Mã này sẽ hết hạn sau 5 phút. Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email.</p>
                <hr style='border: 0; border-top: 1px solid #eee;' />
                <p style='text-align: center; font-size: 12px; color: #aaa;'>&copy; 2026 Yash Gems Support Team</p>
            </div>"
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(
                host,
                port,
                MailKit.Security.SecureSocketOptions.StartTls
            );

            await client.AuthenticateAsync(senderEmail, appPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            Console.WriteLine("--> EMAIL GỬI VÀO MAILTRAP THÀNH CÔNG!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> LỖI GỬI EMAIL: {ex.Message}");
        }
    }
}
