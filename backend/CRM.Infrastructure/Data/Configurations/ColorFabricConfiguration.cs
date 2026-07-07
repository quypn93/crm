using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class ColorFabricConfiguration : IEntityTypeConfiguration<ColorFabric>
{
    public void Configure(EntityTypeBuilder<ColorFabric> builder)
    {
        builder.ToTable("ColorFabrics");

        builder.HasKey(cf => cf.Id);

        builder.Property(cf => cf.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cf => cf.Description)
            .HasMaxLength(500);

        builder.HasIndex(cf => cf.Name);

        // Màu ăn theo chất liệu: FK optional, xoá chất liệu thì gỡ liên kết (SET NULL).
        builder.HasOne(cf => cf.Material)
            .WithMany()
            .HasForeignKey(cf => cf.MaterialId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(cf => cf.MaterialId);
    }
}
