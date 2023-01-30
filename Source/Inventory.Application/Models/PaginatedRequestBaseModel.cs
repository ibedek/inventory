namespace Inventory.Application.Models;

public class PaginatedRequestBaseModel
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}