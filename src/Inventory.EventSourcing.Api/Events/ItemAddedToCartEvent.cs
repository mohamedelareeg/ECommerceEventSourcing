
// Events
public record ItemAddedToCartEvent(Guid UserId, Guid SelectedProductId, string ProductName, int Quantity, decimal UnitPrice);

