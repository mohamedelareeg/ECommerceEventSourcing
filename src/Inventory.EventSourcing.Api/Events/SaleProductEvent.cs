public record SaleProductEvent(Guid Id, Guid ProductId, Guid WarehouseId, int QuantitySold, double Price, int NewQuantity);

