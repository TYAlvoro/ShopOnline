using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OrderService.Data;

namespace OrderService.Tests;

public sealed class OrderServiceTests
{
    private readonly OrdersDbContext _databaseContext =
        new(new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private readonly OrderService.Services.OrderService _orderService;

    public OrderServiceTests()
    {
        _orderService = new(_databaseContext, NullLogger<OrderService.Services.OrderService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ShouldWriteOrderAndOutboxMessage()
    {
        var userId = Guid.NewGuid();

        await _orderService.CreateAsync(userId, 50m);

        _databaseContext.Orders.Should().ContainSingle();
        _databaseContext.Outbox.Should().ContainSingle();
    }
}