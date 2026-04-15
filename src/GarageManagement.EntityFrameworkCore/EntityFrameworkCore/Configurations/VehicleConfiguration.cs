using GarageManagement.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace GarageManagement.EntityFrameworkCore.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable(GarageManagementConsts.DbTablePrefix + "Vehicles", GarageManagementConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(x => x.Make)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Model)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.LicensePlate)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(x => x.LicensePlate).IsUnique();
    }
}
