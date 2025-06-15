using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using ShopOnline.Shared.Contracts;
using ShopOnline.Shared.Outbox;

namespace OrderService.Services;

public sealed class OrderService(
    OrdersDbContext databaseContext,
    ILogger<OrderService> logger)
    : IOrderService
{
    public async Task<Order> CreateAsync(
        Guid userId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var order = new Order { UserId = userId, Amount = amount };
        databaseContext.Orders.Add(order);

        var outboxMessage = new OutboxMessage
        {
            Type = nameof(OrderCreated),
            Payload = JsonSerializer.Serialize(
                new OrderCreated(order.Id, order.UserId, order.Amount))
        };
        databaseContext.Outbox.Add(outboxMessage);

        await databaseContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Order {OrderId} persisted", order.Id);

        return order;
    }

    public async Task<IReadOnlyList<Order>> ListAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await databaseContext.Orders
            .Where(order => order.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task<Order?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        databaseContext.Orders.FindAsync([id], cancellationToken).AsTask();
}