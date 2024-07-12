public record OrderCreatedEvent(Guid OrderId, Guid UserId, List<CartItemDto> Items);

