using GarageManagement.ServiceOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace GarageManagement.EntityFrameworkCore.Configurations;

public class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
{
    public void Configure(EntityTypeBuilder<ServiceOrder> builder)
    {
        builder.ToTable(GarageManagementConsts.DbTablePrefix + "ServiceOrders", GarageManagementConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(x => x.ServiceOrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(1000);

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.Property(x => x.VehicleId)
            .IsRequired();

        builder.HasIndex(x => x.ServiceOrderNumber).IsUnique();
    }
}
