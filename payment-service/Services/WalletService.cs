using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;

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
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive");

        var wallet = await databaseContext.Wallets.FindAsync([userId], cancellationToken);

        if (wallet is null)
        {
            wallet = new Wallet { UserId = userId, Balance = amount };
            databaseContext.Wallets.Add(wallet);
        }
        else
        {
            wallet.Balance += amount;
        }

        databaseContext.Transactions.Add(new Transaction
        {
            UserId = userId,
            Amount = amount
        });

        await databaseContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Wallet {UserId} credited by {Amount}", userId, amount);
    }

    public async Task<decimal> GetBalanceAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var wallet = await databaseContext.Wallets.FindAsync([userId], cancellationToken);
        return wallet?.Balance ?? 0m;
    }

    public async Task<bool> TryCreateWalletAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (await databaseContext.Wallets.AnyAsync(w => w.UserId == userId, cancellationToken))
            return false;

        databaseContext.Wallets.Add(new Wallet { UserId = userId, Balance = 0 });
        await databaseContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}