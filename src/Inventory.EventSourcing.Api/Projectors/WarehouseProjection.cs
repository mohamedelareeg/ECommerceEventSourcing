using Inventory.EventSourcing.Api.Models;
using Marten.Events.Projections;

namespace Inventory.EventSourcing.Api.Projectors
{
    public class WarehouseProjection : MultiStreamAggregation<Warehouse, Guid>
    {
        public WarehouseProjection()
        {
            //Identity<TransferProductBetweenWarehousesEvent>(x => x.SourceWarehouseId);
            //Identity<TransferProductBetweenWarehousesEvent>(x => x.TargetWarehouseId);
        }

        //public void Apply(Warehouse warehouse, TransferProductBetweenWarehousesEvent e)
        //{
        //    if (warehouse.Id == e.SourceWarehouseId)
        //    {
        //        var product = warehouse.Products.FirstOrDefault(p => p.ProductId == e.ProductId);
        //        if (product != null)
        //        {
        //            product.Quantity -= e.Quantity;
        //        }
        //    }
        //    else if (warehouse.Id == e.TargetWarehouseId)
        //    {
        //        var product = warehouse.Products.FirstOrDefault(p => p.ProductId == e.ProductId);
        //        if (product == null)
        //        {
        //            product = new WarehouseProduct
        //            {
        //                ProductId = e.ProductId,
        //                Quantity = e.Quantity,
        //            };
        //            warehouse.Products.Add(product);
        //        }
        //        else
        //        {
        //            product.Quantity += e.Quantity;
        //        }
        //    }
        //}
    }
}
