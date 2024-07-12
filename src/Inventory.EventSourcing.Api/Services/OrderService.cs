using Inventory.EventSourcing.Api.Services.Abstractions;
using Marten;
using static OrderSummaryProjector;

namespace Inventory.EventSourcing.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly IDocumentSession _session;

        public OrderService(IDocumentSession session)
        {
            _session = session;
        }

        public async Task<object> CreateOrderAsync(Guid userId)
        {
            var cart = await _session.LoadAsync<Cart>(userId);
            if (cart == null || cart.Items.Count == 0)
            {
                return Results.BadRequest("Cart is empty. Cannot create an order without items.");
            }

            var orderId = Guid.NewGuid();
            var orderItems = cart.Items.Select(item => new CartItemDto
            {
                SelectedProductId = item.SelectedProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList();

            _session.Events.StartStream(new OrderCreatedEvent(orderId, userId, orderItems));

            // Clear cart items after order creation (optional, based on your business logic)
            cart.Items.Clear();
            await _session.SaveChangesAsync();

            return Results.Ok($"Order created with ID: {orderId}");
        }
        public async Task<object> CancelOrderAsync(Guid orderId, Guid userId)
        {
            var order = await _session.LoadAsync<Order>(orderId);
            if (order == null || order.UserId != userId)
            {
                return Results.NotFound($"Order {orderId} not found or you do not have permission to cancel this order.");
            }

            // Append the cancellation event
            _session.Events.Append(orderId, new OrderCancelledEvent(orderId, userId));

            // Optionally, update cart with items from the canceled order
            var cart = await _session.LoadAsync<Cart>(userId);
            if (cart == null)
            {
                cart = new Cart { Id = userId };
                _session.Store(cart);
            }

            foreach (var item in order.Items)
            {
                cart.Items.Add(new CartItemDto
                {
                    SelectedProductId = item.SelectedProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            await _session.SaveChangesAsync();

            return Results.Ok($"Order {orderId} has been cancelled.");
        }
        public async Task<string> SubmitOrderAsync(Guid userId)
        {
            var cart = await _session.LoadAsync<Cart>(userId);
            if (cart == null || cart.Items.Count == 0)
            {
                return "Cart is empty. Cannot submit an order without items.";
            }

            // Check if all items in cart are available in inventory
            foreach (var cartItem in cart.Items)
            {
                var product = await _session.LoadAsync<Product>(cartItem.SelectedProductId);
                if (product == null || product.Quantity < cartItem.Quantity)
                {
                    return $"Not enough quantity available for product {cartItem.SelectedProductId}. Available: {product?.Quantity ?? 0}";
                }
            }

            var orderId = Guid.NewGuid();
            var orderItems = cart.Items.Select(item => new CartItemDto
            {
                SelectedProductId = item.SelectedProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList();

            _session.Events.StartStream(new OrderCreatedEvent(orderId, userId, orderItems));

            // Optionally, clear cart items after order submission (based on your business logic)
            cart.Items.Clear();
            await _session.SaveChangesAsync();

            return $"Order submitted successfully with ID: {orderId}";
        }

        public async Task<List<OrderDto>> GetOrdersAsync(Guid userId)
        {
            var orders = await _session.Query<Order>().Where(o => o.UserId == userId).ToListAsync();

            if (orders == null || !orders.Any())
            {
                throw new InvalidOperationException("No orders found for this user.");
            }

            var orderDtos = orders.Select(order => new OrderDto
            {
                OrderId = order.Id,
                Items = order.Items,
                Total = order.CalculateTotalPrice(),
                ShippingAddress = order.ShippingAddress,
                ShippingPhoneNumber = order.ShippingPhoneNumber
            }).ToList();

            return orderDtos;
        }
    }

}
