using Inventory.EventSourcing.Api.Events;
using Inventory.EventSourcing.Api.Models;
using Inventory.EventSourcing.Api.Services.Abstractions;
using Marten;
using Microsoft.AspNetCore.Http;

namespace Inventory.EventSourcing.Api.Services
{
    public class ProductTransactionService : IProductTransactionService
    {
        private readonly IDocumentSession _session;

        public ProductTransactionService(IDocumentSession session)
        {
            _session = session;
        }

        public async Task<string> SaleProductAsync(Guid warehouseId, Guid productId, int quantity, double price)
        {
            var latestTransaction = _session.Query<ProductTransaction>()
              .Where(x => x.ProductId == productId && x.WarehouseId == warehouseId)
              .OrderByDescending(x => x.CreatedDate)
              .FirstOrDefault();
            if (latestTransaction == null)
            {
                throw new InvalidOperationException($"Product with ID {productId} not found in Warehouse {warehouseId}.");
            }

            var newQuantity = latestTransaction.CurrentQuantity - quantity;

            if (newQuantity < 0)
            {
                throw new InvalidOperationException($"Not enough quantity available for product {productId} in Warehouse {warehouseId}. Available: {latestTransaction.CurrentQuantity}");
            }

            //var lastSaleTimestamp = _session.Query<ProductTransaction>()
            //   .Where(x => x.ProductId == productId && x.TransactionType == "Sale")
            //   .Max(x => x.CreatedDate);

            //var purchases = _session.Query<ProductTransaction>()
            //    .Where(x => x.ProductId == productId && x.TransactionType == "Purchase" && x.CreatedDate > lastSaleTimestamp)
            //    .OrderBy(x => x.CreatedDate)
            //    .ToList();

            //decimal totalCost = 0;
            //int remainingQuantity = quantity;

            //foreach (var purchase in purchases)
            //{
            //    if (remainingQuantity <= purchase.QuantityChanged)
            //    {
            //        totalCost += remainingQuantity * (decimal)purchase.LastPrice;
            //        break;
            //    }
            //    else
            //    {
            //        totalCost += purchase.QuantityChanged * (decimal)purchase.LastPrice;
            //        remainingQuantity -= purchase.QuantityChanged;
            //    }
            //}

            //decimal totalRevenue = quantity * (decimal)price;
            //decimal profit = totalRevenue - totalCost;
            _session.Events.StartStream(new SaleProductEvent(Guid.NewGuid(), productId, warehouseId, quantity, price, newQuantity));

            await _session.SaveChangesAsync();

            return $"Sale Product {productId} added to inventory.";
        }

        public async Task<string> PurchaseProductAsync(Guid warehouseId, Guid productId, int quantity, double price)
        {
            var latestTransaction = _session.Query<ProductTransaction>()
              .Where(x => x.ProductId == productId && x.WarehouseId == warehouseId)
              .OrderByDescending(x => x.CreatedDate)
              .FirstOrDefault();

            var newQuantity = 0;
            if (latestTransaction != null)
            {
                newQuantity = latestTransaction.CurrentQuantity + quantity;
            }
            else
            {
                var warehouse = await _session.Query<Warehouse>().FirstOrDefaultAsync(w => w.Id == warehouseId);
                if (warehouse == null)
                {
                    warehouse = new Warehouse { Id = warehouseId };
                    _session.Store(warehouse);
                    await _session.SaveChangesAsync();
                }
                var product = await _session.LoadAsync<Product>(productId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product with ID {productId} not found.");
                }
            }

            if (newQuantity < 0)
            {
                throw new InvalidOperationException($"Not enough quantity available for product {productId} in Warehouse {warehouseId}. Available: {latestTransaction.CurrentQuantity}");
            }
            _session.Events.StartStream(new PurchaseProductEvent(Guid.NewGuid(), productId, warehouseId, quantity, price, newQuantity));

            await _session.SaveChangesAsync();

            return $"Purchase Product {productId} added to inventory.";
        }
        public async Task<string> TransferProductBetweenWarehousesAsync(Guid sourceWarehouseId, Guid targetWarehouseId, Guid productId, int quantity)
        {
            var sourceWarehouse = await _session.LoadAsync<Warehouse>(sourceWarehouseId);
            var targetWarehouse = await _session.LoadAsync<Warehouse>(targetWarehouseId);

            if (sourceWarehouse == null || targetWarehouse == null)
            {
                throw new InvalidOperationException($"One or both warehouses not found.");
            }

            var sourceProduct = _session.Query<ProductTransaction>().OrderByDescending(x => x.CreatedDate).FirstOrDefault(p => p.ProductId == productId && p.WarehouseId == sourceWarehouseId);
            if (sourceProduct == null || sourceProduct.CurrentQuantity < quantity)
            {
                throw new InvalidOperationException($"Not enough quantity available in the source warehouse for product {productId}.");
            }

            sourceProduct.CurrentQuantity -= quantity;

            var targetProduct = _session.Query<ProductTransaction>().OrderByDescending(x => x.CreatedDate).FirstOrDefault(p => p.ProductId == productId && p.WarehouseId == targetWarehouseId);
            if (targetProduct == null || targetProduct.CurrentQuantity < quantity)
            {
                throw new InvalidOperationException($"Not enough quantity available in the Target warehouse for product {productId}.");
            }

            targetProduct.CurrentQuantity += quantity;

            _session.Events.StartStream(new TransferProductBetweenWarehousesEvent(Guid.NewGuid(), sourceWarehouseId, targetWarehouseId, productId, quantity));

            await _session.SaveChangesAsync();

            return $"Transferred {quantity} of product {productId} from warehouse {sourceWarehouseId} to {targetWarehouseId}.";
        }
    }
}
