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
        modelBuilder.Entity<Order>(b =>
        {
            b.ToTable("orders");
            b.HasKey(o => o.Id);
            b.Property(o => o.Id).HasColumnName("id");
            b.Property(o => o.UserId).HasColumnName("userid");
            b.Property(o => o.Amount).HasColumnName("amount");
            b.Property(o => o.Status).HasColumnName("status");
            b.Property(o => o.CreatedAt).HasColumnName("createdat");
        });

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("outbox");
            b.HasKey(m => m.Id);
            b.Property(m => m.Id).HasColumnName("id");
            b.Property(m => m.OccurredOn).HasColumnName("occurredon");
            b.Property(m => m.Type).HasColumnName("type");
            b.Property(m => m.Payload).HasColumnName("payload");
            b.Property(m => m.ProcessedAt).HasColumnName("processedat");
        });
    }
}