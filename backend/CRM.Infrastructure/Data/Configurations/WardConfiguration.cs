using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class WardConfiguration : IEntityTypeConfiguration<Ward>
{
    public void Configure(EntityTypeBuilder<Ward> builder)
    {
        builder.ToTable("Wards");

        builder.HasKey(w => w.Code);

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(w => w.FullName)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(w => w.Type)
            .HasMaxLength(50);

        builder.Property(w => w.ProvinceCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasOne(w => w.Province)
            .WithMany(p => p.Wards)
            .HasForeignKey(w => w.ProvinceCode)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.ProvinceCode);
        builder.HasIndex(w => w.Name);
    }
}
