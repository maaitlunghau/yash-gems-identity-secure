using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using YashGems.Identity.Application.DTOs.Messaging;
using YashGems.Identity.Worker.Services;

namespace YashGems.Identity.Worker;

public class MessageSubscriber : BackgroundService
{
    private readonly ILogger<MessageSubscriber> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly IEmailService _emailService;
    private readonly string _exchangeName = "YashGemsExchange";
    private readonly string _queueName = "IdentityOtpQueue";

    public MessageSubscriber(
        ILogger<MessageSubscriber> logger,
        IConfiguration configuration,
        IEmailService emailService
    )
    {
        _logger = logger;
        _configuration = configuration;
        _emailService = emailService;

        InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672")
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        // Khai báo Exchange (giống hệt bên API)
        _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Direct).GetAwaiter().GetResult();

        // Khai báo một cái "Phễu" (Queue) để hứng tin nhắn
        _channel.QueueDeclareAsync(
            _queueName, durable: true,
            exclusive: false,
            autoDelete: false
        ).GetAwaiter().GetResult();

        // Nối cái Phễu này vào Nhà ga (Exchange) thông qua cái Nhãn (RoutingKey)
        _channel.QueueBindAsync(
            _queueName,
            _exchangeName,
            "otp-routing-key")
        .GetAwaiter().GetResult();

        _logger.LogInformation(
            "--> Worker đang lắng nghe ở hàng đợi: {QueueName}",
            _queueName
        );
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var messageString = Encoding.UTF8.GetString(body);
            var otpMsg = JsonSerializer.Deserialize<SendOtpMessage>(messageString);

            if (otpMsg != null)
            {
                _logger.LogInformation("==========================================");
                _logger.LogInformation(
                    "--> [WORKER NHẬN TIN]: Gửi {Type} đến {To}",
                    otpMsg.MessageType,
                    string.IsNullOrEmpty(otpMsg.Email) ? otpMsg.PhoneNumber : otpMsg.Email
                );
                _logger.LogInformation("--> [NỘI DUNG]: MÃ OTP CỦA BẠN LÀ: {Code}", otpMsg.OtpCode);

                await _emailService.SendOtpEmailAsync(otpMsg.Email, otpMsg.OtpCode);
                _logger.LogInformation("--> Đã gửi Email thật thành công đến: {Email}", otpMsg.Email);
            }

            // Gửi xác nhận cho RabbitMQ là "Tôi đã nhận hàng thành công, hãy xóa tin nhắn này đi"
            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
        };

        await _channel!.BasicConsumeAsync(
            queue: _queueName,
            autoAck: false,
            consumer: consumer
        );
    }
}
