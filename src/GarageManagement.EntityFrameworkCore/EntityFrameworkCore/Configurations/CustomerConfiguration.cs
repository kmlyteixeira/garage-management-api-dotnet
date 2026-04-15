using GarageManagement.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace GarageManagement.EntityFrameworkCore.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable(GarageManagementConsts.DbTablePrefix + "Customers", GarageManagementConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Document)
            .HasConversion(
                document => document.Value,
                value => new Document(value))
            .HasColumnName("Document")
            .IsRequired()
            .HasMaxLength(14);

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.Document).IsUnique();
    }
}
