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
    private readonly string _otpQueue = "IdentityOtpQueue";
    private readonly string _kycQueue = "IdentityKycQueue";

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

        // Khai báo OTP Queue
        _channel.QueueDeclareAsync(_otpQueue, durable: true, exclusive: false, autoDelete: false).GetAwaiter().GetResult();
        _channel.QueueBindAsync(_otpQueue, _exchangeName, "otp-routing-key").GetAwaiter().GetResult();

        // Khai báo KYC Queue
        _channel.QueueDeclareAsync(_kycQueue, durable: true, exclusive: false, autoDelete: false).GetAwaiter().GetResult();
        _channel.QueueBindAsync(_kycQueue, _exchangeName, "kyc-email-routing-key").GetAwaiter().GetResult();

        _logger.LogInformation("--> Worker đang lắng nghe mượt mà các queues...");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        // OTP Consumer
        var otpConsumer = new AsyncEventingBasicConsumer(_channel!);
        otpConsumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var messageString = Encoding.UTF8.GetString(body);
            try {
                var otpMsg = JsonSerializer.Deserialize<SendOtpMessage>(messageString);
                if (otpMsg != null)
                {
                    await _emailService.SendOtpEmailAsync(otpMsg.Email, otpMsg.OtpCode, otpMsg.MessageType);
                    _logger.LogInformation("--> Đã gửi OTP Email thành công đến: {Email}", otpMsg.Email);
                }
            } catch(Exception ex) {
                _logger.LogError(ex, "Error processing OTP message");
            }
            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
        };
        await _channel!.BasicConsumeAsync(queue: _otpQueue, autoAck: false, consumer: otpConsumer);

        // KYC Consumer
        var kycConsumer = new AsyncEventingBasicConsumer(_channel!);
        kycConsumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var messageString = Encoding.UTF8.GetString(body);
            try {
                var kycMsg = JsonSerializer.Deserialize<SendKycStatusMessage>(messageString);
                if (kycMsg != null)
                {
                    await _emailService.SendKycStatusEmailAsync(kycMsg.Email, kycMsg.Status);
                    _logger.LogInformation("--> Đã gửi KYC Email [{Status}] thành công đến: {Email}", kycMsg.Status, kycMsg.Email);
                }
            } catch(Exception ex) {
                _logger.LogError(ex, "Error processing KYC message");
            }
            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
        };
        await _channel!.BasicConsumeAsync(queue: _kycQueue, autoAck: false, consumer: kycConsumer);
    }
}
