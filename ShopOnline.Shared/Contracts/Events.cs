namespace ShopOnline.Shared.Contracts;

public record OrderCreated(Guid OrderId, Guid UserId, decimal Amount);

public record PaymentCompleted(Guid OrderId);

public record PaymentFailed(Guid OrderId, string Reason);