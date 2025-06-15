using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace ShopOnline.Shared.Messaging;

public sealed class KafkaProducer(IConfiguration configuration) : IKafkaProducer
{
    private readonly IProducer<string, string> _producer = new ProducerBuilder<string, string>(
        new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            EnableIdempotence = true
        }).Build();

    public Task PublishAsync(string topic,
        string key,
        string value,
        CancellationToken cancellationToken = default)
        => _producer.ProduceAsync(
            topic,
            new Message<string, string> { Key = key, Value = value },
            cancellationToken);

    public void Dispose()
    {
        _producer.Flush();
        _producer.Dispose();
    }
}