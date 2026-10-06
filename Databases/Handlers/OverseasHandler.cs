namespace Handlers;
using Models;
using RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MySqlContext;
using System.Text;
using System.Text.Json;
using Elastic.Clients.Elasticsearch;

public class OverseasHandler : IAlertHandler
{
    private readonly ElasticsearchClient _es;
    private readonly Context _context;
    public OverseasHandler (Context context, string elasticUri)
    {
        _context = context;
        _es = new ElasticsearchClient(new ElasticsearchClientSettings(new Uri(elasticUri)));
    }
    public async Task HandleAsync()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync("OVERSEAS", durable: true, exclusive: false, autoDelete: false);
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
            var result = await channel.BasicConsumeAsync("OVERSEAS", autoAck: true, consumer: consumer);
            var alert = JsonSerializer.Deserialize<Alert>(result);
            if (alert != null)
            {
                if (alert.priority == "CRITICAL")
                {
                    await _es.IndexAsync(new {
                    Level = "Warning",
                    Source = "Overseas db handler ",
                    Content = "notice! a critical alert has arrived",
                    Timestamp = DateTime.Now
                    }, x => x.Index("logs").Id(1));
                }
                _context.NorthAlerts.Add(alert);
                continue;
            }
            await _es.IndexAsync(new {
                    Level = "Warning",
                    Source = "Overseas db handler ",
                    Content = "failed to read alert",
                    Timestamp = DateTime.Now
                    }, x => x.Index("logs").Id(1));
        }
    }
}