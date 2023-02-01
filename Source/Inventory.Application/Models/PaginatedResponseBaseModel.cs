namespace Inventory.Application.Models;

public record PaginatedResponseBaseModel<T>
{
    public int TotalResultCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<T> Results { get; set; } = default!;
}