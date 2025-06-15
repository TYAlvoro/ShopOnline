using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using ShopOnline.Shared.Contracts;
using ShopOnline.Shared;
using ShopOnline.Shared.Outbox;
using System.Text.Json;

namespace OrderService.Messaging;

public sealed class KafkaConsumer(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<KafkaConsumer> logger) : BackgroundService
{
    private const string SourceTopic = Topics.OrdersStatus;
    private readonly IConsumer<string, string> _consumer =
        new ConsumerBuilder<string, string>(
            new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = "orders-svc",
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
        var messageId = Guid.Parse(result.Message.Key);

        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        if (await db.Inbox.AnyAsync(i => i.Id == messageId, cancellationToken))
            return;

        using var doc = JsonDocument.Parse(result.Message.Value);
        var root = doc.RootElement;
        var orderId = root.GetProperty("OrderId").GetGuid();
        var isFailed = root.TryGetProperty("Reason", out _);

        var order = await db.Orders.FindAsync([orderId], cancellationToken);
        if (order is null)
            return;

        order.Status = isFailed ? OrderStatus.Failed : OrderStatus.Paid;

        db.Inbox.Add(new InboxMessage { Id = messageId, Topic = SourceTopic });
        db.Outbox.Add(new OutboxMessage
        {
            Type = isFailed ? nameof(PaymentFailed) : nameof(PaymentCompleted),
            Payload = result.Message.Value
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}