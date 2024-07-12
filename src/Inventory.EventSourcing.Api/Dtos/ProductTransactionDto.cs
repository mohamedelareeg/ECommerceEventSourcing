namespace Inventory.EventSourcing.Api.Dtos
{
    public class ProductTransactionDto
    {
        public Guid Id { get; set; }
        public int QuantityChanged { get; set; }
        public int CurrentQuantity { get; set; }
        public double LastPrice { get; set; }
        public string TransactionType { get; set; }
    }
}
