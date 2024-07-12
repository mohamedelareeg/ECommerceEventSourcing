public class Cart
{
    public Guid Id { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public string ShippingAddress { get; set; }
    public string ShippingPhoneNumber { get; set; }
    public decimal CalculateCartTotal()
    {
        decimal total = 0;
        foreach (var item in Items)
        {
            total += item.Quantity * item.UnitPrice;
        }
        return total;
    }
}

