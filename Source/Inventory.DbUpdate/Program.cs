using Inventory.Persistence;
using Microsoft.EntityFrameworkCore;

var connectionString = args.FirstOrDefault() ?? "Server=.\\SQLExpress;Database=inventory;Integrated Security=true;Trust Server Certificate=true;";
var dbContextFactory = new InventoryDbContextFactory();
var dbContext = dbContextFactory.CreateDbContext(connectionString);

dbContext.Database.Migrate();

return 0;