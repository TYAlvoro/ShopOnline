namespace OrderService.Models;

public sealed class InboxMessage
{
    public Guid Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}