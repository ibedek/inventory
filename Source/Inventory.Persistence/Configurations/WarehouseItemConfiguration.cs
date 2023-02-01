using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Persistence.Configurations;

public class WarehouseItemConfiguration : IEntityTypeConfiguration<WarehouseItem>
{
    public void Configure(EntityTypeBuilder<WarehouseItem> builder)
    {
        builder.HasOne(x => x.Warehouse)
            .WithMany(x => x.WarehouseItems)
            .HasForeignKey(x => x.WarehouseId);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.WarehouseItems)
            .HasForeignKey(x => x.ProductId);
    }
}