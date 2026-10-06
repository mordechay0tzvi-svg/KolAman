using Models;
using System.Text;
namespace Handlers;
using MySqlContext;
using RabbitMQ.Client;
using System.Text.Json;
using RabbitMQ.Client.Events;
using Elastic.Clients.Elasticsearch;
public class CenterHandler : IAlertHandler
{
    private readonly ElasticsearchClient _es;
    private readonly Context _context;
    public CenterHandler (Context context, string elasticUri)
    {
        _context = context;
        _es = new ElasticsearchClient(new ElasticsearchClientSettings(new Uri(elasticUri)));
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
            {if (alert.priority == "CRITICAL")
                {
                    await _es.IndexAsync(new {
                    Level = "Warning",
                    Source = "Center db handler ",
                    Content = "notice! a critical alert has arrived",
                    Timestamp = DateTime.Now
                    }, x => x.Index("logs").Id(1));
                }
                _context.CenterAlerts.Add(alert);
                continue;
            }
            await _es.IndexAsync(new {
                    Level = "Warning",
                    Source = "Center db handler ",
                    Content = "failed to read alert",
                    Timestamp = DateTime.Now
                    }, x => x.Index("logs").Id(1));
        }
    }
}