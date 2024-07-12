namespace Inventory.EventSourcing.Api.Dtos
{
    public class ProductDetailsDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public int CurrentQuantity { get; set; }
        public List<ProductTransactionDto> Transactions { get; set; }
    }
}
