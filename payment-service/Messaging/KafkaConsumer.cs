using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;
using ShopOnline.Shared.Contracts;
using ShopOnline.Shared.Outbox;
using System.Text.Json;
using ShopOnline.Shared;

namespace PaymentService.Messaging;

public sealed class KafkaConsumer(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<KafkaConsumer> logger) : BackgroundService
{
    private const string SourceTopic = Topics.OrdersPayments;
    public readonly string StatusTopic = Topics.OrdersStatus;
    private readonly IConsumer<string, string> _consumer =
        new ConsumerBuilder<string, string>(
            new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = "payments-svc",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            }).Build();

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(SourceTopic);

        return Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    await ProcessMessageAsync(consumeResult, stoppingToken);
                    _consumer.Commit(consumeResult);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing Kafka message");
                }
            }
        }, stoppingToken);
    }

    private async Task ProcessMessageAsync(
        ConsumeResult<string, string> result,
        CancellationToken cancellationToken)
    {
        // idempotency key = message key (GUID из OrderService.Outbox)
        var messageId = Guid.Parse(result.Message.Key);

        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

        // уже обработали?
        if (await db.Inbox.AnyAsync(i => i.Id == messageId, cancellationToken))
            return;

        var orderCreated = JsonSerializer.Deserialize<OrderCreated>(result.Message.Value)!;

        // business logic
        var wallet = await db.Wallets.FindAsync([orderCreated.UserId], cancellationToken);
        var success = wallet is not null && wallet.Balance >= orderCreated.Amount;

        db.Inbox.Add(new InboxMessage { Id = messageId, Topic = SourceTopic });

        if (success)
        {
            wallet!.Balance -= orderCreated.Amount;
            db.Outbox.Add(new OutboxMessage
            {
                Type = nameof(PaymentCompleted),
                Payload = JsonSerializer.Serialize(new PaymentCompleted(orderCreated.OrderId))
            });
        }
        else
        {
            db.Outbox.Add(new OutboxMessage
            {
                Type = nameof(PaymentFailed),
                Payload = JsonSerializer.Serialize(
                    new PaymentFailed(orderCreated.OrderId, "Insufficient funds"))
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}
