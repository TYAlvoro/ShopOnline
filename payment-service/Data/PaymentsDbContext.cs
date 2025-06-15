using Microsoft.EntityFrameworkCore;
using PaymentService.Models;
using ShopOnline.Shared.Outbox;

namespace PaymentService.Data;

public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
    : DbContext(options), IOutboxDbContext
{
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Wallet>().HasKey(wallet => wallet.UserId);
        modelBuilder.Entity<OutboxMessage>().ToTable("outbox");
    }
}