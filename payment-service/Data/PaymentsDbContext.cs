using Microsoft.EntityFrameworkCore;
using PaymentService.Models;
using ShopOnline.Shared.Outbox;

namespace PaymentService.Data;

public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
    : DbContext(options), IOutboxDbContext
{
    public DbSet<InboxMessage> Inbox => Set<InboxMessage>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Wallet>(b =>
        {
            b.ToTable("wallets");
            b.HasKey(w => w.UserId);
            b.Property(w => w.UserId).HasColumnName("userid");
            b.Property(w => w.Balance).HasColumnName("balance");
        });

        modelBuilder.Entity<Transaction>(b =>
        {
            b.ToTable("transactions");
            b.HasKey(t => t.Id);
            b.Property(t => t.Id).HasColumnName("id");
            b.Property(t => t.UserId).HasColumnName("userid");
            b.Property(t => t.Amount).HasColumnName("amount");
            b.Property(t => t.OccurredAt).HasColumnName("occurredat");
        });

        modelBuilder.Entity<InboxMessage>(b =>
        {
            b.ToTable("inbox");
            b.HasKey(i => i.Id);
            b.Property(i => i.Id).HasColumnName("id");
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