using Microsoft.AspNetCore.SignalR;
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
            var db = scope.ServiceProvider.GetRequiredService<TDatabaseContext>();
            var hub = scope.ServiceProvider.GetService<IHubContext<Hub>>();

            var messages = await db.Outbox
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.OccurredOn)
                .Take(50)
                .ToListAsync(cancellationToken);

            foreach (var message in messages)
            {
                await kafkaProducer.PublishAsync(
                    _topic,
                    message.Id.ToString(),
                    message.Payload,
                    cancellationToken);

                if (hub is not null)
                    await hub.Clients.All.SendAsync(message.Type, message.Payload, cancellationToken);

                message.ProcessedAt = DateTime.UtcNow;
            }

            if (messages.Count > 0)
                await db.SaveChangesAsync(cancellationToken);

            await Task.Delay(800, cancellationToken);
        }
    }
}
