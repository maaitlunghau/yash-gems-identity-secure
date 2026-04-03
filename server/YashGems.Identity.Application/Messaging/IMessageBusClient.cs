namespace YashGems.Identity.Application.Messaging;

public interface IMessageBusClient
{
    // định nghĩa một phương thức chung để gửi bất kỳ Model nào vào RabbitMQ
    void PublishNewMessage(object message, string routingKey);
}
