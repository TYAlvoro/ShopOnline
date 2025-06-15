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
    public async Task DepositAsync_ShouldIncreaseBalance()
    {
        var userId = Guid.NewGuid();

        await _walletService.DepositAsync(userId, 100m);

        (await _walletService.GetBalanceAsync(userId)).Should().Be(100m);
    }
    
    [Fact]
    public async Task TryCreateWallet_ShouldReturnFalse_WhenWalletExists()
    {
        var userId = Guid.NewGuid();
        await _walletService.TryCreateWalletAsync(userId);
        (await _walletService.TryCreateWalletAsync(userId)).Should().BeFalse();
    }

    [Fact]
    public async Task DepositAsync_ShouldFail_WhenNegativeAmount()
    {
        var userId = Guid.NewGuid();
        await _walletService.Invoking(s => s.DepositAsync(userId, -10m))
            .Should()
            .ThrowAsync<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public async Task DepositAsync_ShouldHandleConcurrentDeposits()
    {
        var databaseName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var ctx1 = new PaymentsDbContext(options);
        var ctx2 = new PaymentsDbContext(options);
        var service1 = new WalletService(ctx1, NullLogger<WalletService>.Instance);
        var service2 = new WalletService(ctx2, NullLogger<WalletService>.Instance);

        var userId = Guid.NewGuid();

        await Task.WhenAll(
            service1.DepositAsync(userId, 50m),
            service2.DepositAsync(userId, 70m));

        await using var finalContext = new PaymentsDbContext(options);
        var finalService = new WalletService(finalContext, NullLogger<WalletService>.Instance);

        (await finalService.GetBalanceAsync(userId)).Should().Be(120m);
    }
}