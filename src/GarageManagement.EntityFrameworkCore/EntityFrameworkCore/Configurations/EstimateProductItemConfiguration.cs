using GarageManagement.Estimates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace GarageManagement.EntityFrameworkCore.Configurations;

public class EstimateProductItemConfiguration : IEntityTypeConfiguration<EstimateProductItem>
{
    public void Configure(EntityTypeBuilder<EstimateProductItem> builder)
    {
        builder.ToTable(GarageManagementConsts.DbTablePrefix + "EstimateProductItems", GarageManagementConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.TotalPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasIndex(x => new { x.EstimateId, x.ProductId });
    }
}
