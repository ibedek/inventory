namespace Inventory.Core.Entities;

public class WarehouseItem
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    public long SerialNumber { get; set; }
}