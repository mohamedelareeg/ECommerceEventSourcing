using Inventory.EventSourcing.Api.Events;
using Inventory.EventSourcing.Api.Models;
using Marten;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Http;

namespace Inventory.EventSourcing.Api.Projectors
{
    public class ProductTransactionProjection : MultiStreamAggregation<ProductTransaction, Guid>
    {

        public ProductTransactionProjection()
        {
            Identity<SaleProductEvent>(x => x.Id);
            //Identity<SaleProductEvent>(x => x.ProductId);
            //Identity<SaleProductEvent>(x => x.WarehouseId);
            Identity<PurchaseProductEvent>(x => x.Id);
            //Identity<PurchaseProductEvent>(x => x.ProductId);
            //Identity<PurchaseProductEvent>(x => x.WarehouseId);
            Identity<TransferProductBetweenWarehousesEvent>(x => x.SourceWarehouseId);
            //Identity<TransferProductBetweenWarehousesEvent>(x => x.ProductId);
            Identity<TransferProductBetweenWarehousesEvent>(x => x.TargetWarehouseId);
        }

        public void Apply(ProductTransaction product, SaleProductEvent e)
        {
            product.WarehouseId = e.WarehouseId;
            product.ProductId = e.ProductId;
            product.QuantityChanged = e.QuantitySold;
            product.CurrentQuantity = e.NewQuantity;
            product.LastPrice = e.Price;
            product.TransactionType = "Sale";
        }

        public void Apply(ProductTransaction product, PurchaseProductEvent e)
        {
            product.WarehouseId = e.WarehouseId;
            product.ProductId = e.ProductId;
            product.QuantityChanged = e.QuantityPurchased;
            product.CurrentQuantity = e.NewQuantity;
            product.LastPrice = e.Price;
            product.TransactionType = "Purchase";

        }
        public void Apply(ProductTransaction product, TransferProductBetweenWarehousesEvent e)
        {
            if (product.WarehouseId == e.SourceWarehouseId)
            {
                product.WarehouseId = e.TargetWarehouseId;
                product.ProductId = e.ProductId;
                product.QuantityChanged = e.Quantity;
                product.CurrentQuantity -= e.Quantity;
                product.TransactionType = "Transfer Out";
            }

            if (product.WarehouseId == e.TargetWarehouseId)
            {
                product.WarehouseId = e.SourceWarehouseId;
                product.ProductId = e.ProductId;
                product.QuantityChanged = e.Quantity;
                product.CurrentQuantity += e.Quantity;
                product.TransactionType = "Transfer In";
            }
        }

    }

}
