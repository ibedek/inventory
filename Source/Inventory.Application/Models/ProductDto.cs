namespace Inventory.Application.Models;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public long ItemReference { get; set; }
    public string CompanyName { get; set; }
    public long CompanyPrefix { get; set; }
}