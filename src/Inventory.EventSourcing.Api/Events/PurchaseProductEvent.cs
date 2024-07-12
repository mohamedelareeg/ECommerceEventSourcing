public record PurchaseProductEvent(Guid Id, Guid ProductId, Guid WarehouseId, int QuantityPurchased, double Price, int NewQuantity);

