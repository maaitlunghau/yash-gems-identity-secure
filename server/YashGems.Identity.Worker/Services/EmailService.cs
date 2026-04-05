using MailKit.Net.Smtp;
using MimeKit;

namespace YashGems.Identity.Worker.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
        => _configuration = configuration;

    public async Task SendOtpEmailAsync(string toEmail, string otpCode, string messageType = "Registration")
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var message = new MimeMessage();

        var senderName = emailSettings["SenderName"] ?? throw new ArgumentNullException();
        var senderEmail = emailSettings["SenderEmail"] ?? throw new ArgumentNullException();
        var host = emailSettings["Host"] ?? throw new ArgumentNullException();
        var port = int.Parse(emailSettings["Port"] ?? throw new ArgumentNullException());
        var appPassword = emailSettings["AppPassword"] ?? throw new ArgumentNullException();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress("", toEmail));

        string subject = "Mã xác thực OTP của bạn - Yash Gems";
        string heading = "YASH GEMS IDENTITY";
        string description = "Bạn vừa yêu cầu mã xác thực để đăng nhập hoặc đăng ký tại hệ thống.";
        if (messageType == "ForgotPassword")
        {
            subject = "YASH GEMS - Yêu cầu đặt lại mật khẩu";
            heading = "PHỤC HỒI MẬT KHẨU";
            description = "Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Vui lòng sử dụng mã dưới đây:";
        }
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
            <h2 style='color: #d4af37; text-align: center; border-bottom: 2px solid #d4af37; padding-bottom: 10px;'>{heading}</h2>
            <p style='color: #333; font-size: 16px;'>Chào bạn,</p>
            <p style='color: #555;'>{description}</p>
            <div style='background-color: #f9f9f9; padding: 20px; text-align: center; border-radius: 8px; margin: 20px 0;'>
                <span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #222;'>{otpCode}</span>
            </div>
            <p style='color: #888; font-size: 13px;'>Mã này sẽ hết hạn sau 5 phút. <b>Lưu ý:</b> Không chia sẻ mã này cho bất kỳ ai để bảo mật tài khoản.</p>
            <hr style='border: 0; border-top: 1px solid #eee; margin-top: 25px;' />
            <p style='text-align: center; font-size: 12px; color: #aaa;'>&copy; 2026 Yash Gems Luxury Support Team</p>
        </div>"
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(senderEmail, appPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            Console.WriteLine($"--> GỬI EMAIL [{messageType}] THÀNH CÔNG ĐẾN: {toEmail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> LỖI GỬI EMAIL: {ex.Message}");
        }
    }

    public async Task SendKycStatusEmailAsync(string toEmail, string status)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var message = new MimeMessage();

        var senderName = emailSettings["SenderName"] ?? throw new ArgumentNullException();
        var senderEmail = emailSettings["SenderEmail"] ?? throw new ArgumentNullException();
        var host = emailSettings["Host"] ?? throw new ArgumentNullException();
        var port = int.Parse(emailSettings["Port"] ?? throw new ArgumentNullException());
        var appPassword = emailSettings["AppPassword"] ?? throw new ArgumentNullException();
        
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress("", toEmail));

        string subject = "Kết quả xác thực eKYC - Yash Gems";
        string heading = "THÔNG BÁO XÁC THỰC eKYC";
        string description = "";
        string messageColor = "#555";
        
        if (status == "Verified")
        {
            subject = "🎉 Chúc mừng! Tài khoản của bạn đã xác thực eKYC thành công!";
            description = "Chúc mừng bạn! Dữ liệu khuôn mặt và giấy tờ của bạn đã khớp hoàn toàn. Bạn hiện đã trở thành khách hàng định danh tại Yash Gems và có thể thoải mái đặt mua những sản phẩm trang sức, kim cương giá trị cao mà không gặp giới hạn nào.";
            messageColor = "#16a34a"; // green
        }
        else if (status == "Rejected")
        {
            subject = "⚠️ Xác thực eKYC thất bại - Vui lòng thử lại";
            description = "Dữ liệu khuôn mặt hoặc giấy tờ của bạn không hợp lệ hoặc bức ảnh quá mờ để AI có thể nhận diện. Vui lòng quay lại hệ thống để thực hiện quét lại.";
            messageColor = "#dc2626"; // red
        }
        else if (status == "Pending")
        {
            subject = "⏳ Dữ liệu eKYC đang chờ duyệt";
            description = "Dữ liệu xác thực của bạn đã được tải lên thành công và đang trong quá trình chờ Quản trị viên (Admin) xét duyệt thủ công. Vui lòng chờ thông báo mới nhất từ chúng tôi.";
            messageColor = "#d97706"; // amber
        }

        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
            <h2 style='color: #d4af37; text-align: center; border-bottom: 2px solid #d4af37; padding-bottom: 10px;'>{heading}</h2>
            <p style='color: #333; font-size: 16px;'>Chào bạn,</p>
            <div style='background-color: #f9f9f9; padding: 20px; border-left: 4px solid {messageColor}; border-radius: 4px; margin: 20px 0;'>
                <p style='color: {messageColor}; font-size: 15px; margin: 0; line-height: 1.5;'>{description}</p>
            </div>
            <p style='text-align: center; font-size: 12px; color: #aaa; margin-top: 30px;'>&copy; 2026 Yash Gems Luxury Support Team</p>
        </div>"
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(senderEmail, appPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            Console.WriteLine($"--> GỬI EMAIL KYC [{status}] THÀNH CÔNG ĐẾN: {toEmail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> LỖI GỬI EMAIL KYC: {ex.Message}");
        }
    }
}
