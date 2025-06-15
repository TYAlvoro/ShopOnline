using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShopOnline.Shared.Messaging;

namespace ShopOnline.Shared.Outbox;

public sealed class OutboxPublisher<TDatabaseContext, THub>(
    IServiceProvider serviceProvider,
    IKafkaProducer kafkaProducer,
    IConfiguration configuration,
    ILogger<OutboxPublisher<TDatabaseContext, THub>> logger)
    : BackgroundService
    where TDatabaseContext : DbContext, IOutboxDbContext
    where THub : Hub
{
    private readonly string _topic =
        configuration["Kafka:OutboxTopic"] ?? Topics.OrdersPayments;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = serviceProvider.CreateAsyncScope();
                var databaseContext = scope.ServiceProvider.GetRequiredService<TDatabaseContext>();
                var hubContext = scope.ServiceProvider.GetService<IHubContext<THub>>();

                var outboxMessages = await databaseContext.Outbox
                    .Where(message => message.ProcessedAt == null)
                    .OrderBy(message => message.OccurredOn)
                    .Take(50)
                    .ToListAsync(cancellationToken);

                foreach (var message in outboxMessages)
                {
                    await kafkaProducer.PublishAsync(
                        _topic,
                        message.Id.ToString(),
                        message.Payload,
                        cancellationToken);

                    if (hubContext is not null)
                        await hubContext.Clients.All.SendAsync(
                            message.Type,
                            message.Payload,
                            cancellationToken);

                    message.ProcessedAt = DateTime.UtcNow;
                }

                if (outboxMessages.Count > 0)
                    await databaseContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Outbox publishing failed, retrying");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(800), cancellationToken);
        }
    }
}
