using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Models;
namespace Kafka;
public class Producer
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topicname;
    public Producer(IConfiguration configuration)
    {
        var config = new ProducerConfig{BootstrapServers = configuration["Kafka:BootstrapServers"]};
        _producer = new ProducerBuilder<string, string>(config).Build();
        _topicname = configuration["Kafka:TopicName"] ?? "alerts";
    }
    public async Task<DeliveryResult<string, string>> SendAsync(Alert alert)
    {
        var message = new Message<string, string>
        {
            Key = alert.alert_id,
            Value = JsonSerializer.Serialize(alert)
        };
        return await _producer.ProduceAsync(_topicname, message);
    }
} 