using Microsoft.AspNetCore.Mvc;
using PaymentService.Services;

namespace PaymentService.Controllers;

[ApiController]
[Route("payments")]
public sealed class PaymentsController(IWalletService walletService) : ControllerBase
{
    public record DepositRequest(decimal Amount);

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit(
        [FromHeader(Name = "user_id")] Guid userId,
        [FromBody] DepositRequest request)
    {
        await walletService.DepositAsync(userId, request.Amount);
        return Accepted();
    }

    [HttpGet("balance")]
    public async Task<decimal> Balance(
        [FromHeader(Name = "user_id")] Guid userId) =>
        await walletService.GetBalanceAsync(userId);
    
    [HttpPost("account")]
    public async Task<IActionResult> CreateWallet(
        [FromHeader(Name = "user_id")] Guid userId)
    {
        var created = await walletService.TryCreateWalletAsync(userId);
        return created ? CreatedAtAction(nameof(Balance), new { userId }, null)
            : Conflict("Wallet already exists");
    }
}