namespace PaymentService.Models;

public sealed class Wallet
{
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public uint Version { get; set; }
}