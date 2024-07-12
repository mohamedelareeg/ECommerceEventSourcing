namespace Inventory.EventSourcing.Api.Services.Abstractions
{
    public interface IProductTransactionService
    {
        Task<string> SaleProductAsync(Guid warehouseId, Guid productId, int quantity, double price);
        Task<string> PurchaseProductAsync(Guid warehouseId, Guid productId, int quantity, double price);
        Task<string> TransferProductBetweenWarehousesAsync(Guid sourceWarehouseId, Guid targetWarehouseId, Guid productId, int quantity);

    }
}
