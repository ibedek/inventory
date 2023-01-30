namespace Inventory.Core.Entities;

public record Company : AuditableEntity
{
    public Company()
    {
        
    }
    
    public Company(DateTime createdAt, string createdBy)
    {
        Id = Guid.NewGuid();
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }
    
    public long Prefix { get; set; }
    public string Name { get; set; } = default!;
}