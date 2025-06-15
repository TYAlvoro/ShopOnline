using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShopOnline.Shared.Messaging;

namespace ShopOnline.Shared.Outbox;

public sealed class OutboxPublisher<TDatabaseContext>(
    IServiceProvider serviceProvider,
    IKafkaProducer kafkaProducer,
    IConfiguration configuration,
    ILogger<OutboxPublisher<TDatabaseContext>> logger)
    : BackgroundService
    where TDatabaseContext : DbContext, IOutboxDbContext
{
    private readonly string _topic =
        configuration["Kafka:OutboxTopic"] ?? Topics.OrdersPayments;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var databaseContext = scope.ServiceProvider.GetRequiredService<TDatabaseContext>();

            var outboxMessages = await databaseContext.Outbox
                .Where(outboxMessage => outboxMessage.ProcessedAt == null)
                .OrderBy(outboxMessage => outboxMessage.OccurredOn)
                .Take(30)
                .ToListAsync(cancellationToken);

            foreach (var outboxMessage in outboxMessages)
            {
                await kafkaProducer.PublishAsync(
                    _topic,
                    outboxMessage.Id.ToString(),
                    outboxMessage.Payload,
                    cancellationToken);

                outboxMessage.ProcessedAt = DateTime.UtcNow;
            }

            if (outboxMessages.Count > 0)
            {
                await databaseContext.SaveChangesAsync(cancellationToken);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(800), cancellationToken);
        }
    }
}