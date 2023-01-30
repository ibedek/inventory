using Inventory.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasAlternateKey(x => x.Prefix);
        builder.HasMany(x => x.Products)
            .WithOne(x => x.Company)
            .HasForeignKey(x => x.CompanyId);
    }
}