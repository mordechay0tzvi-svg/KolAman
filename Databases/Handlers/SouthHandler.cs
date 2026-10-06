namespace Handlers;
using Models;
using RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MySqlContext;
using System.Text;
using System.Text.Json;
using MongoDB.Bson;

public class SouthHandler : IAlertHandler
{
    private readonly Context _context;
    public SouthHandler (Context context)
    {
        _context = context;
    }
    public async Task HandleAsync()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync("SOUTH", durable: true, exclusive: false, autoDelete: false);
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [x] Received {message}");
            return Task.CompletedTask;
        };
        while (true)
        {
            var result = await channel.BasicConsumeAsync("SOUTH", autoAck: true, consumer: consumer);
            var alert = JsonSerializer.Deserialize<Alert>(result.ToJson());
            if (alert != null)
            {
                _context.SouthAlerts.Add(alert);
            }
        }
    }
}