using Inventory.EventSourcing.Api.Models;

namespace Inventory.EventSourcing.Api.Services.Abstractions
{
    public interface IWarehouseService
    {
        //Task<List<WarehouseProduct>> GetWarehouseProductsAsync(Guid warehouseId);
        Task<List<Warehouse>> GetAllWarehousesAsync();

    }
}
