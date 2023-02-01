namespace Inventory.Core.Entities;

public record Warehouse : AuditableEntity
{
    public Warehouse()
    {
        WarehouseItems = new HashSet<WarehouseItem>();
    }

    public string Code { get; set; } = default!;
    public string Location { get; set; } = default!;

    public virtual ICollection<WarehouseItem> WarehouseItems { get; set; }
}