using GarageManagement.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace GarageManagement.EntityFrameworkCore.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable(GarageManagementConsts.DbTablePrefix + "Products", GarageManagementConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Price)
            .IsRequired()
            .HasPrecision(18, 2);
    }
}
