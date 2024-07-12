namespace Inventory.EventSourcing.Api.Events
{
    public record TransferProductBetweenWarehousesEvent(Guid Id, Guid SourceWarehouseId, Guid TargetWarehouseId, Guid ProductId, int Quantity);
}
