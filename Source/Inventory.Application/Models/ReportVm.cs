namespace Inventory.Application.Models;

public class ReportVm
{
    public long ProductReference { get; set; }
    public string ProductName { get; set; } = default!;
    public int TotalCount { get; set; }
    public DateTime DateOfInventory { get; set; }
    public long CompanyPrefix { get; set; }
    public string CompanyName { get; set; } = default!;
}