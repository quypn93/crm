using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class SenderAddressConfiguration : IEntityTypeConfiguration<SenderAddress>
{
    public void Configure(EntityTypeBuilder<SenderAddress> builder)
    {
        builder.ToTable("SenderAddresses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Phone).IsRequired().HasMaxLength(30);
        builder.Property(x => x.Address).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ProvinceName).HasMaxLength(150);
        builder.Property(x => x.DistrictName).HasMaxLength(150);
        builder.Property(x => x.WardName).HasMaxLength(150);
    }
}
