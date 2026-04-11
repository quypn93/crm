using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class ShirtComponentConfiguration : IEntityTypeConfiguration<ShirtComponent>
{
    public void Configure(EntityTypeBuilder<ShirtComponent> builder)
    {
        builder.ToTable("ShirtComponents");

        builder.HasKey(sc => sc.Id);

        builder.Property(sc => sc.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sc => sc.ImageUrl)
            .HasMaxLength(500);

        builder.Property(sc => sc.WomenImageUrl)
            .HasMaxLength(500);

        builder.Property(sc => sc.IsDeleted)
            .HasDefaultValue(false);

        builder.HasOne(sc => sc.ColorFabric)
            .WithMany(cf => cf.ShirtComponents)
            .HasForeignKey(sc => sc.ColorFabricId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(sc => sc.Type);
        builder.HasIndex(sc => sc.ColorFabricId);
        builder.HasIndex(sc => sc.IsDeleted);
    }
}
