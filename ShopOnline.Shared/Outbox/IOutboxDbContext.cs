using Microsoft.EntityFrameworkCore;

namespace ShopOnline.Shared.Outbox;

public interface IOutboxDbContext
{
    DbSet<OutboxMessage> Outbox { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}