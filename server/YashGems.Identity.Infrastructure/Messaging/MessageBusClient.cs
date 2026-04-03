using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using YashGems.Identity.Application.Messaging;

namespace YashGems.Identity.Infrastructure.Messaging;

public class MessageBusClient : IMessageBusClient
{
    private readonly IConfiguration _configuration;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _exchangeName;

    public MessageBusClient(IConfiguration configuration)
    {
        _configuration = configuration;
        _exchangeName = "YashGemsExchange";

        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672")
        };

        try
        {
            // thiết lập kết nối 
            // (đồng bộ trong Constructor để đảm bảo có kết nối trước khi chạy)
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            // khai báo Exchange (dạng Direct)
            _channel.ExchangeDeclareAsync(
                exchange: _exchangeName,
                type: ExchangeType.Direct
            ).GetAwaiter().GetResult();

            Console.WriteLine("--> Kết nối thành công đến RabbitMQ Message Bus");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Không thể kết nối đến RabbitMQ: {ex.Message}");
        }
    }

    public void PublishNewMessage(object message, string routingKey)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        _channel.BasicPublishAsync(
            exchange: _exchangeName,
            routingKey: routingKey,
            body: body
        ).GetAwaiter().GetResult();

        Console.WriteLine($"--> Đã gửi message đến RabbitMQ: {routingKey}");
    }
}
