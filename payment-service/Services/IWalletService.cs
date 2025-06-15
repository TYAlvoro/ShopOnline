namespace PaymentService.Services;

public interface IWalletService
{
    Task DepositAsync(Guid userId,
        decimal amount,
        CancellationToken cancellationToken = default);

    Task<decimal> GetBalanceAsync(Guid userId,
        CancellationToken cancellationToken = default);
}