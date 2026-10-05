namespace Rabbit;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;

public class RabbitMqService 
{
    private readonly IConnection _connection;
    public RabbitMqService(IConfiguration configuration)
    {
        var host = configuration["Rabbit:host"] ?? "localhost";
        var user = configuration["Rabbit:user"] ?? "guest";
        var factory = new ConnectionFactory
        {
            HostName = host,
            UserName = user
        };
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
    }

    public async Task ConsumeAsync(string queue)
    {
        var channel = await _connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(
            queue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                await channel.BasicAckAsync(
                    ea.DeliveryTag,
                    multiple: false);
            }
            catch
            {
                await channel.BasicNackAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
        };
        await channel.BasicConsumeAsync(
            queue: queue,
            autoAck: false,
            consumer: consumer);
    }
}