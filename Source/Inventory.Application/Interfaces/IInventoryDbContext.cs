using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Interfaces;

public interface IInventoryDbContext
{
    DbSet<Company> Companies { get; set; }
    DbSet<Product> Products { get; set; }
    DbSet<Warehouse> Warehouses { get; set; }
    DbSet<WarehouseItem> WarehouseItems { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}