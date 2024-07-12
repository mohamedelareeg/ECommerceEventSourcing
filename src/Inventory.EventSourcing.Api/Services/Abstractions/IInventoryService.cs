using Inventory.EventSourcing.Api.Dtos;

namespace Inventory.EventSourcing.Api.Services.Abstractions
{
    public interface IInventoryService
    {
        Task<string> AddProductToInventoryAsync(string name, Guid warehouseId);
        Task<string> RemoveProductFromInventoryAsync(Guid productId, Guid warehouseId);
        Task<string> UpdateProductInfoAsync(Guid productId, Guid warehouseId, string name);
        Task<ProductDetailsDto> GetProductDetailsAsync(Guid productId, Guid warehouseId);
    }

}
