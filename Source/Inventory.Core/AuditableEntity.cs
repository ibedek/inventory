namespace Inventory.Core;

public record AuditableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; } = default!;
}