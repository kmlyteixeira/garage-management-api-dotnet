using GarageManagement.Estimates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace GarageManagement.EntityFrameworkCore.Configurations;

public class EstimateConfiguration : IEntityTypeConfiguration<Estimate>
{
    public void Configure(EntityTypeBuilder<Estimate> builder)
    {
        builder.ToTable(GarageManagementConsts.DbTablePrefix + "Estimates", GarageManagementConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(x => x.EstimateNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.EstimateNumber).IsUnique();

        builder.HasMany(x => x.ServiceItems)
            .WithOne()
            .HasForeignKey(x => x.EstimateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PartItems)
            .WithOne()
            .HasForeignKey(x => x.EstimateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
