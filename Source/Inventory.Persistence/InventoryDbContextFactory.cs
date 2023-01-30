using Microsoft.EntityFrameworkCore;

namespace Inventory.Persistence;

public class InventoryDbContextFactory : DesignTimeDbContextFactoryBase<InventoryDbContext>
{
    protected override InventoryDbContext CreateNewInstance(DbContextOptions<InventoryDbContext> options)
    {
        return new InventoryDbContext(options);
    }
}