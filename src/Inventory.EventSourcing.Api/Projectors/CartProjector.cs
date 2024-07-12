using Marten;
using Marten.Events.Projections;
public class CartProjector : MultiStreamAggregation<Cart, Guid>
{
    public CartProjector()
    {
        Identity<ItemAddedToCartEvent>(x => x.UserId);
        Identity<ItemRemovedFromCartEvent>(x => x.UserId);
        Identity<ItemQuantityUpdatedEvent>(x => x.UserId);
        Identity<ItemPriceUpdatedEvent>(x => x.UserId);
        Identity<ShippingInformationUpdatedEvent>(x => x.UserId);
    }

    public void Apply(Cart cart, ItemAddedToCartEvent e, IQuerySession querySession)
    {
        var product = querySession.Load<Product>(e.SelectedProductId);

        if (product != null && product.Quantity >= e.Quantity)
        {
            var existingItem = cart.Items.FirstOrDefault(item => item.SelectedProductId == e.SelectedProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += e.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItemDto
                {
                    SelectedProductId = e.SelectedProductId,
                    ProductName = e.ProductName,
                    Quantity = e.Quantity,
                    UnitPrice = e.UnitPrice,
                });
            }
        }
        else
        {
            throw new InvalidOperationException($"Product {e.SelectedProductId} not found or does not have enough quantity available.");
        }
    }


    public void Apply(Cart cart, ItemRemovedFromCartEvent e)
    {
        var existingItem = cart.Items.FirstOrDefault(item => item.SelectedProductId == e.SelectedProductId);
        if (existingItem != null)
        {
            existingItem.Quantity -= e.Quantity;
            if (existingItem.Quantity <= 0)
            {
                cart.Items.Remove(existingItem);
            }
        }
    }

    public void Apply(Cart cart, ItemQuantityUpdatedEvent e)
    {
        var existingItem = cart.Items.FirstOrDefault(item => item.SelectedProductId == e.SelectedProductId);
        if (existingItem != null)
        {
            existingItem.Quantity = e.Quantity;
        }
    }

    public void Apply(Cart cart, ItemPriceUpdatedEvent e)
    {
        var existingItem = cart.Items.FirstOrDefault(item => item.SelectedProductId == e.SelectedProductId);
        if (existingItem != null)
        {
            existingItem.UnitPrice = e.Price;
        }
    }

    public void Apply(Cart cart, ShippingInformationUpdatedEvent e)
    {
        cart.ShippingAddress = e.Address;
        cart.ShippingPhoneNumber = e.PhoneNumber;
    }
}

