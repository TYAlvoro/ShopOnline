using Microsoft.AspNetCore.Mvc;
using PaymentService.Services;

namespace PaymentService.Controllers;

[ApiController]
[Route("payments")]
public sealed class PaymentsController(IWalletService walletService) : ControllerBase
{
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit(
        [FromHeader(Name = "user_id")] Guid userId,
        [FromBody] decimal amount)
    {
        await walletService.DepositAsync(userId, amount);
        return Accepted();
    }

    [HttpGet("balance")]
    public async Task<decimal> Balance(
        [FromHeader(Name = "user_id")] Guid userId) =>
        await walletService.GetBalanceAsync(userId);
}