using Inventory.EventSourcing.Api.Models;
using Inventory.EventSourcing.Api.Services.Abstractions;
using Marten;

namespace Inventory.EventSourcing.Api.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IDocumentSession _session;

        public WarehouseService(IDocumentSession session)
        {
            _session = session;
        }

        //public async Task<List<WarehouseProduct>> GetWarehouseProductsAsync(Guid warehouseId)
        //{
        //    var warehouse = await _session.Query<Warehouse>().FirstOrDefaultAsync(w => w.Id == warehouseId);
        //    return warehouse?.Products ?? new List<WarehouseProduct>();
        //}
        public async Task<List<Warehouse>> GetAllWarehousesAsync()
        {
            return _session.Query<Warehouse>().ToList();
        }

    }
}
