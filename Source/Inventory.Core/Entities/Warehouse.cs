namespace Inventory.Core.Entities;

public record Warehouse : AuditableEntity
{
    public Warehouse()
    {
        WarehouseItems = new HashSet<WarehouseItem>();
    }
    
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Location { get; set; }
    
    public virtual ICollection<WarehouseItem> WarehouseItems { get; set; }
}