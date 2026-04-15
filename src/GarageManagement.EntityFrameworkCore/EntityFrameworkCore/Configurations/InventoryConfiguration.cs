using GarageManagement.Inventories;
using GarageManagement.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace GarageManagement.EntityFrameworkCore.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable(GarageManagementConsts.DbTablePrefix + "Inventories", GarageManagementConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.HasOne<Product>(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ProductId).IsUnique();
    }
}
