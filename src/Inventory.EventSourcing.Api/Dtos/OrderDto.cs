public class OrderDto
{
    public Guid OrderId { get; set; }
    public List<CartItemDto> Items { get; set; }
    public decimal Total { get; set; }
    public string ShippingAddress { get; set; }
    public string ShippingPhoneNumber { get; set; }
}

