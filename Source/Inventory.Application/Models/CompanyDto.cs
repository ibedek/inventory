namespace Inventory.Application.Models;

public class CompanyDto
{
    public Guid Id { get; set; }
    public long Prefix { get; set; }
    public string Name { get; set; } = default!;
}