using OrderService.Models;

namespace OrderService.Services;

public interface IOrderService
{
    Task<Order> CreateAsync(Guid userId,
        decimal amount,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> ListAsync(Guid userId,
        CancellationToken cancellationToken = default);

    Task<Order?> GetAsync(Guid id,
        CancellationToken cancellationToken = default);
}