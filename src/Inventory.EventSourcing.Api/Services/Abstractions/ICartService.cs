namespace Inventory.EventSourcing.Api.Services.Abstractions
{
    public interface ICartService
    {
        Task<string> AddItemToCartAsync(Guid userId, Guid productId, string productName, decimal price);
        Task<string> RemoveItemFromCartAsync(Guid userId, Guid productId);
        Task<string> UpdateCartItemQuantityAsync(Guid userId, Guid productId, int quantity);
        Task<string> UpdateCartItemPriceAsync(Guid userId, Guid productId, decimal price);
        Task<string> UpdateShippingInformationAsync(Guid userId, string address, string phoneNumber);
        Task<string> CreateOrderAsync(Guid userId);
        Task<string> CancelOrderAsync(Guid orderId, Guid userId);
        Task<(List<CartItemDto> cartItems, decimal total)> GetCartDetailsAsync(Guid userId);
        Task<List<EventDto>> GetCartEventStreamAsync(Guid userId);
    }
}
