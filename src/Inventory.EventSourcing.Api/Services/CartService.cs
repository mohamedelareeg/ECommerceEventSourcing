using Inventory.EventSourcing.Api.Services.Abstractions;
using Marten;

namespace Inventory.EventSourcing.Api.Services
{
    public class CartService : ICartService
    {
        private readonly IDocumentSession _session;

        public CartService(IDocumentSession session)
        {
            _session = session;
        }

        public async Task<string> AddItemToCartAsync(Guid userId, Guid productId, string productName, decimal price)
        {
            var product = await _session.LoadAsync<Product>(productId);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {productId} not found.");
            }

            // Validate input data
            if (string.IsNullOrEmpty(productName))
            {
                throw new ArgumentException("Product name cannot be empty.");
            }
            if (price <= 0)
            {
                throw new ArgumentException("Price must be greater than zero.");
            }

            // Append event to the user's stream
            _session.Events.Append(
                userId,
                new ItemAddedToCartEvent(userId, productId, productName, 1, price)
            );

            await _session.SaveChangesAsync();

            return $"Item with Product ID {productId}, Name {productName}, and Price {price} added to cart!";
        }


        public async Task<string> RemoveItemFromCartAsync(Guid userId, Guid productId)
        {
            _session.Events.Append(
                userId,
                new ItemRemovedFromCartEvent(userId, productId, 1)
            );

            await _session.SaveChangesAsync();

            return $"Item with Product ID {productId} removed from cart!";
        }

        public async Task<string> UpdateCartItemQuantityAsync(Guid userId, Guid productId, int quantity)
        {
            _session.Events.Append(
                userId,
                new ItemQuantityUpdatedEvent(userId, productId, quantity)
            );

            await _session.SaveChangesAsync();

            return $"Item with Product ID {productId} quantity updated to {quantity}!";
        }

        public async Task<string> UpdateCartItemPriceAsync(Guid userId, Guid productId, decimal price)
        {
            _session.Events.Append(
                userId,
                new ItemPriceUpdatedEvent(userId, productId, price)
            );

            await _session.SaveChangesAsync();

            return $"Item with Product ID {productId} price updated to {price}!";
        }

        public async Task<string> UpdateShippingInformationAsync(Guid userId, string address, string phoneNumber)
        {
            _session.Events.Append(
                userId,
                new ShippingInformationUpdatedEvent(userId, address, phoneNumber)
            );

            await _session.SaveChangesAsync();

            return "Shipping information updated!";
        }

        public async Task<string> CreateOrderAsync(Guid userId)
        {
            var orderId = Guid.NewGuid();
            var cart = await _session.LoadAsync<Cart>(userId);
            if (cart == null || cart.Items.Count == 0)
            {
                return "Cart is empty. Cannot create an order without items.";
            }

            var orderItems = cart.Items.Select(item => new CartItemDto
            {
                SelectedProductId = item.SelectedProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList();

            _session.Events.StartStream(new OrderCreatedEvent(orderId, userId, orderItems));

            // Clear cart items after order creation (optional, depends on your business logic)
            cart.Items.Clear();
            await _session.SaveChangesAsync();

            return $"Order created with ID: {orderId}";
        }

        public async Task<string> CancelOrderAsync(Guid orderId, Guid userId)
        {
            _session.Events.Append(
                userId,
                new OrderCancelledEvent(orderId, userId)
            );

            await _session.SaveChangesAsync();

            return $"Order {orderId} cancelled!";
        }

        public async Task<(List<CartItemDto> cartItems, decimal total)> GetCartDetailsAsync(Guid userId)
        {
            var cart = await _session.LoadAsync<Cart>(userId);
            if (cart == null)
            {
                throw new InvalidOperationException("Cart not found.");
            }

            return (cart.Items, cart.CalculateCartTotal());
        }

        public async Task<List<EventDto>> GetCartEventStreamAsync(Guid userId)
        {
            var events = await _session.Events.FetchStreamAsync(userId);

            if (events == null || !events.Any())
            {
                throw new InvalidOperationException("No events found for this cart.");
            }

            var eventDtos = events.Select(e => new EventDto
            {
                Type = e.EventType.Name,
                Data = e.Data,
                Date = e.Timestamp.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            return eventDtos;
        }
    }

}
