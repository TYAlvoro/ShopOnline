using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PaymentService.Data;
using PaymentService.Services;
using Xunit;

namespace PaymentService.Tests;

public sealed class WalletServiceTests
{
    private readonly PaymentsDbContext _databaseContext =
        new(new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private readonly WalletService _walletService;

    public WalletServiceTests()
    {
        _walletService = new(_databaseContext, NullLogger<WalletService>.Instance);
    }

    [Fact]
    public async Task DepositAsync_ShouldIncreaseBalance_AndWriteOutboxMessage()
    {
        var userId = Guid.NewGuid();

        await _walletService.DepositAsync(userId, 100m);

        (await _walletService.GetBalanceAsync(userId)).Should().Be(100m);
        _databaseContext.Outbox.Should().ContainSingle();
    }
    
    [Fact]
    public async Task TryCreateWallet_ShouldReturnFalse_WhenWalletExists()
    {
        var userId = Guid.NewGuid();
        await _walletService.TryCreateWalletAsync(userId);
        (await _walletService.TryCreateWalletAsync(userId)).Should().BeFalse();
    }

}