using Models;
using System.Text;
namespace Handlers;
using MySqlContext;
using RabbitMQ.Client;
using System.Text.Json;
using RabbitMQ.Client.Events;
using MongoDB.Bson;

public class CenterHandler : IAlertHandler
{
    private readonly Context _context;
    public CenterHandler (Context context)
    {
        _context = context;
    }
    public async Task HandleAsync()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync("CENTER", durable: true, exclusive: false, autoDelete: false);
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
            var result = await channel.BasicConsumeAsync("CENTER", autoAck: true, consumer: consumer);
            var alert = JsonSerializer.Deserialize<Alert>(result);
            if (alert != null)
            {
                _context.CenterAlerts.Add(alert);
            }
        }
    }
}