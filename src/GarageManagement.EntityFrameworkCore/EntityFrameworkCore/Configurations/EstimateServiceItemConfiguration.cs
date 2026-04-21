using GarageManagement.Estimates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace GarageManagement.EntityFrameworkCore.Configurations;

public class EstimateServiceItemConfiguration : IEntityTypeConfiguration<EstimateServiceItem>
{
    public void Configure(EntityTypeBuilder<EstimateServiceItem> builder)
    {
        builder.ToTable(GarageManagementConsts.DbTablePrefix + "EstimateServiceItems", GarageManagementConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.TotalPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasIndex(x => new { x.EstimateId, x.ServiceId });
    }
}
