namespace Inventory.Core.Entities;

public record Product : AuditableEntity
{
    public Product(DateTime createdAt, string createdBy)
    {
        Id = Guid.NewGuid();
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }
    
    public long ReferenceNumber { get; set; }
    public string Name { get; set; } = default!;

    public Guid CompanyId { get; set; }
    public Company Company { get; set; }
}