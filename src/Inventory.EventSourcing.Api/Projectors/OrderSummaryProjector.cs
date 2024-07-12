using Marten.Events.Projections;

public partial class OrderSummaryProjector : MultiStreamAggregation<Order, Guid>
{
    public OrderSummaryProjector()
    {
        Identity<ItemAddedToCartEvent>(x => x.UserId);
        Identity<ItemRemovedFromCartEvent>(x => x.UserId);
        Identity<ItemQuantityUpdatedEvent>(x => x.UserId);
        Identity<ItemPriceUpdatedEvent>(x => x.UserId);
        Identity<ShippingInformationUpdatedEvent>(x => x.UserId);
        Identity<OrderCreatedEvent>(x => x.UserId);
        Identity<OrderCancelledEvent>(x => x.UserId);
    }

    public void Apply(Order order, ItemAddedToCartEvent e)
    {
        if (order.Items == null)
            order.Items = new List<CartItemDto>();

        var existingItem = order.Items.FirstOrDefault(item => item.SelectedProductId == e.SelectedProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += e.Quantity;
        }
        else
        {
            order.Items.Add(new CartItemDto
            {
                SelectedProductId = e.SelectedProductId,
                ProductName = e.ProductName,
                Quantity = e.Quantity,
                UnitPrice = e.UnitPrice
            });
        }
    }

    public void Apply(Order order, ItemRemovedFromCartEvent e)
    {
        if (order.Items == null)
            order.Items = new List<CartItemDto>();

        var existingItem = order.Items.FirstOrDefault(item => item.SelectedProductId == e.SelectedProductId);
        if (existingItem != null)
        {
            existingItem.Quantity -= e.Quantity;
            if (existingItem.Quantity <= 0)
            {
                order.Items.Remove(existingItem);
            }
        }
    }

    public void Apply(Order order, ItemQuantityUpdatedEvent e)
    {
        if (order.Items == null)
            order.Items = new List<CartItemDto>();

        var existingItem = order.Items.FirstOrDefault(item => item.SelectedProductId == e.SelectedProductId);
        if (existingItem != null)
        {
            existingItem.Quantity = e.Quantity;
        }
    }

    public void Apply(Order order, ItemPriceUpdatedEvent e)
    {
        if (order.Items == null)
            order.Items = new List<CartItemDto>();

        var existingItem = order.Items.FirstOrDefault(item => item.SelectedProductId == e.SelectedProductId);
        if (existingItem != null)
        {
            existingItem.UnitPrice = e.Price;
        }
    }
    public void Apply(Order order, ShippingInformationUpdatedEvent e)
    {
        order.ShippingAddress = e.Address;
        order.ShippingPhoneNumber = e.PhoneNumber;
    }
    public void Apply(Order order, OrderCreatedEvent e)
    {
        order.Id = e.OrderId;
        order.UserId = e.UserId;
        order.Items = e.Items;
    }

    public void Apply(Order order, OrderCancelledEvent e)
    {
        order.IsCancelled = true;
    }
   
}

