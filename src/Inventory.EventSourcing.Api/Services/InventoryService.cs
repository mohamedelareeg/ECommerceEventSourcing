using Inventory.EventSourcing.Api.Dtos;
using Inventory.EventSourcing.Api.Models;
using Inventory.EventSourcing.Api.Services.Abstractions;
using Marten;
using Newtonsoft.Json;
using static OrderSummaryProjector;

namespace Inventory.EventSourcing.Api.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IDocumentSession _session;

        public InventoryService(IDocumentSession session)
        {
            _session = session;
        }

        public async Task<string> AddProductToInventoryAsync(string name, Guid warehouseId)
        {
            var productExistInWarhouse = await _session.Query<Product>()
                                       .FirstOrDefaultAsync(p => p.WarehouseId == warehouseId && p.Name == name);

            if (productExistInWarhouse != null)
            {
                return $"Product {name} already exists in the inventory for warehouse {warehouseId}.";
            }
            var existingProduct = await _session.Query<Product>()
                                     .FirstOrDefaultAsync(p => p.Name == name);
            var productId = Guid.NewGuid();
            if (existingProduct != null)
            {
                productId = existingProduct.ProductId;
            }
            var warehouse = await _session.Query<Warehouse>().FirstOrDefaultAsync(w => w.Id == warehouseId);
            if (warehouse == null)
            {
                warehouse = new Warehouse { Id = warehouseId };
                _session.Store(warehouse);
                await _session.SaveChangesAsync();
            }
            _session.Events.Append(productId, new ProductAddedEvent(Guid.NewGuid(), productId, warehouseId, name));
            await _session.SaveChangesAsync();
            return $"Product {name} added to inventory.";
        }

        public async Task<string> RemoveProductFromInventoryAsync(Guid productId, Guid warehouseId)
        {
            var warehouse = await _session.Query<Warehouse>().FirstOrDefaultAsync(w => w.Id == warehouseId);
            if (warehouse == null)
            {
                warehouse = new Warehouse { Id = warehouseId };
                _session.Store(warehouse);
                await _session.SaveChangesAsync();
            }
            _session.Events.Append(Guid.NewGuid(), new ProductRemovedEvent(productId, warehouseId));
            await _session.SaveChangesAsync();
            return $"Product with ID {productId} removed from inventory.";
        }

        public async Task<string> UpdateProductInfoAsync(Guid productId, Guid warehouseId, string name)
        {
            var warehouse = await _session.Query<Warehouse>().FirstOrDefaultAsync(w => w.Id == warehouseId);
            if (warehouse == null)
            {
                warehouse = new Warehouse { Id = warehouseId };
                _session.Store(warehouse);
                await _session.SaveChangesAsync();
            }
            _session.Events.Append(Guid.NewGuid(), new ProductInfoUpdatedEvent(productId, warehouseId, name));
            await _session.SaveChangesAsync();
            return $"Product with ID {productId} updated.";
        }

        public async Task<ProductDetailsDto> GetProductDetailsAsync(Guid productId, Guid warehouseId)
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

            var events = await _session.Query<ProductTransaction>()
                .Where(x => x.ProductId == productId)
                .ToListAsync();

            var lastTransaction = events.LastOrDefault();
            var lastQuantity = lastTransaction?.CurrentQuantity ?? product.Quantity;

            var productDetails = new ProductDetailsDto
            {
                ProductId = productId,
                Name = product.Name,
                CurrentQuantity = lastQuantity,
                Transactions = events.Select(e => new ProductTransactionDto
                {
                    Id = e.Id,
                    QuantityChanged = e.QuantityChanged,
                    CurrentQuantity = e.CurrentQuantity,
                    LastPrice = e.LastPrice,
                    TransactionType = e.TransactionType
                }).ToList()
            };

            return productDetails;
        }


    }

}
