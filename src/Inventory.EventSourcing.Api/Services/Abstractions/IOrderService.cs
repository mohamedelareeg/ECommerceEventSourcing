namespace Inventory.EventSourcing.Api.Services.Abstractions
{
    public interface IOrderService
    {
        Task<object> CreateOrderAsync(Guid userId);
        Task<object> CancelOrderAsync(Guid orderId, Guid userId);
        Task<string> SubmitOrderAsync(Guid userId);
        Task<List<OrderDto>> GetOrdersAsync(Guid userId);
    }
}
