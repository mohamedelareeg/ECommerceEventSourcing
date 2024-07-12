namespace Inventory.EventSourcing.Api.Models
{
    public class ProductTransaction
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid WarehouseId { get; set; }
        public int QuantityChanged { get; set; }
        public int CurrentQuantity { get; set; }
        public double LastPrice { get; set; }
        public string TransactionType { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
