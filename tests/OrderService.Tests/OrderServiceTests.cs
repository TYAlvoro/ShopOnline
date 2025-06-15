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
    
    [Fact]
    public async Task ListAsync_ShouldReturnCreatedOrder()
    {
        var userId = Guid.NewGuid();
        await _orderService.CreateAsync(userId, 10m);
        var list = await _orderService.ListAsync(userId);
        list.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenNotExists()
    {
        (await _orderService.GetAsync(Guid.NewGuid())).Should().BeNull();
    }
    
    [Fact]
    public async Task CreateAsync_ShouldEmitCorrectPayload()
    {
        var userId = Guid.NewGuid();
        var order  = await _orderService.CreateAsync(userId, 42m);

        var msg = _databaseContext.Outbox.Single();
        msg.Payload.Should().Contain(order.Id.ToString());
        msg.Payload.Should().Contain("42");
    }
}