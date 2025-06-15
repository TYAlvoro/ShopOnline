using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;
using ShopOnline.Shared.Contracts;
using ShopOnline.Shared.Outbox;

namespace PaymentService.Services;

public sealed class WalletService(
    PaymentsDbContext databaseContext,
    ILogger<WalletService> logger)
    : IWalletService
{
    public async Task DepositAsync(
        Guid userId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var wallet = await databaseContext.Wallets.FindAsync([userId], cancellationToken)
                     ?? new Wallet { UserId = userId };

        wallet.Balance += amount;
        databaseContext.Wallets.Update(wallet);

        databaseContext.Transactions.Add(new Transaction
        {
            UserId = userId,
            Amount = amount
        });

        databaseContext.Outbox.Add(new OutboxMessage
        {
            Type = nameof(PaymentCompleted),
            Payload = JsonSerializer.Serialize(new PaymentCompleted(Guid.Empty))
        });

        await databaseContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Wallet for user {UserId} credited by {Amount}", userId, amount);
    }

    public async Task<decimal> GetBalanceAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var wallet = await databaseContext.Wallets.FindAsync([userId], cancellationToken);
        return wallet?.Balance ?? 0m;
    }
}