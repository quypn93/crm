using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class DesignConfiguration : IEntityTypeConfiguration<Design>
{
    public void Configure(EntityTypeBuilder<Design> builder)
    {
        builder.ToTable("Designs");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DesignName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Designer)
            .HasMaxLength(200);

        builder.Property(d => d.CustomerFullName)
            .HasMaxLength(200);

        builder.Property(d => d.SizeMan)
            .HasMaxLength(500);

        builder.Property(d => d.SizeWomen)
            .HasMaxLength(500);

        builder.Property(d => d.SizeKid)
            .HasMaxLength(500);

        builder.Property(d => d.Oversized)
            .HasMaxLength(500);

        builder.Property(d => d.NoteOldCodeOrder)
            .HasMaxLength(200);

        builder.Property(d => d.NoteAttachTagLabel)
            .HasMaxLength(500);

        builder.Property(d => d.SaleStaff)
            .HasMaxLength(200);

        builder.HasOne(d => d.ColorFabric)
            .WithMany(cf => cf.Designs)
            .HasForeignKey(d => d.ColorFabricId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.Order)
            .WithMany(o => o.Designs)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.CreatedByUser)
            .WithMany()
            .HasForeignKey(d => d.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(d => d.OrderId);
        builder.HasIndex(d => d.ColorFabricId);
        builder.HasIndex(d => d.CreatedByUserId);
        builder.HasIndex(d => d.CreatedAt);
    }
}
