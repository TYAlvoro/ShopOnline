namespace ShopOnline.Shared.Dtos;

public record CreateOrderDto(decimal Amount, Guid? UserId = null);