using Marten.Events.Projections;
public class InventoryProjector : MultiStreamAggregation<Product, Guid>
{
    public InventoryProjector()
    {
        Identity<ProductAddedEvent>(x => x.Id);
        //Identity<ProductAddedEvent>(x => x.WarehouseId);
        Identity<ProductRemovedEvent>(x => x.ProductId);
        //Identity<ProductRemovedEvent>(x => x.WarehouseId);
        Identity<ProductInfoUpdatedEvent>(x => x.ProductId);
        //Identity<ProductInfoUpdatedEvent>(x => x.WarehouseId);
        Identity<SaleProductEvent>(x => x.ProductId);
        //Identity<SaleProductEvent>(x => x.WarehouseId); 
        Identity<PurchaseProductEvent>(x => x.ProductId);
        //Identity<PurchaseProductEvent>(x => x.WarehouseId);
        //Identity<TransferProductBetweenWarehousesEvent>(x => x.SourceWarehouseId);
        //Identity<TransferProductBetweenWarehousesEvent>(x => x.ProductId);
    }

    public void Apply(Product product, ProductAddedEvent e)
    {
        product.Id = e.Id;
        product.ProductId = e.ProductId;
        product.WarehouseId = e.WarehouseId;
        product.Name = e.Name;
        product.Version++;
    }

    public void Apply(Product product, ProductRemovedEvent e)
    {
        product.Quantity = 0;
        product.Version++;
    }

    public void Apply(Product product, ProductInfoUpdatedEvent e)
    {
        product.Name = e.Name;
    }

    public void Apply(Product product, SaleProductEvent e)
    {
        product.Quantity -= e.QuantitySold;
        product.Version++;
    }

    public void Apply(Product product, PurchaseProductEvent e)
    {
        product.Quantity += e.QuantityPurchased;
        product.Version++;
    }
    //public void Apply(ProductTransaction product, TransferProductBetweenWarehousesEvent e)
    //{
    //    product.CurrentQuantity
    //}
}

