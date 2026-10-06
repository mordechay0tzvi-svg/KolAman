namespace Rabbit;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;

public class RabbitMqService 
{
    private readonly IConnection _connection;
    public RabbitMqService(IConfiguration configuration)
    {
        var host = configuration["Rabbit:Host"] ?? "localhost";
        var factory = new ConnectionFactory
        {
            HostName = host,
        };
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
    }
    public async Task<string> ConsumeAsync(string queue)
    {
        c

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
        return await channel.BasicConsumeAsync(
            queue: queue,
            autoAck: false,
            consumer: consumer);
    }
}