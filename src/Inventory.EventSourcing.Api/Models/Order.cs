public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemDto> Items { get; set; }
    public bool IsCancelled { get; set; }
    public string ShippingAddress { get; set; }
    public string ShippingPhoneNumber { get; set; }

    public decimal CalculateTotalPrice()
    {
        if (Items == null)
            return 0;

        decimal totalPrice = 0;
        foreach (var item in Items)
        {
            totalPrice += item.Quantity * item.UnitPrice;
        }

        return totalPrice;
    }
}

