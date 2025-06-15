using Microsoft.EntityFrameworkCore;
using OrderService.Models;
using ShopOnline.Shared.Outbox;

namespace OrderService.Data;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options)
    : DbContext(options), IOutboxDbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().ToTable("orders");
        modelBuilder.Entity<OutboxMessage>().ToTable("outbox");
    }
}