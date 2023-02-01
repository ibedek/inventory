namespace Inventory.Core.Entities;

public record Product : AuditableEntity
{
    public Product(DateTime createdAt, string createdBy)
    {
        Id = Guid.NewGuid();
        CreatedAt = createdAt;
        CreatedBy = createdBy;
        WarehouseItems = new HashSet<WarehouseItem>();
    }

    public long ReferenceNumber { get; set; }
    public string Name { get; set; } = default!;

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = default!;
    public virtual ICollection<WarehouseItem> WarehouseItems { get; set; }
}