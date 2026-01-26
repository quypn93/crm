using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class DealConfiguration : IEntityTypeConfiguration<Deal>
{
    public void Configure(EntityTypeBuilder<Deal> builder)
    {
        builder.ToTable("Deals");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.Value)
            .HasPrecision(18, 2);

        builder.Property(d => d.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("VND");

        builder.Property(d => d.LostReason)
            .HasMaxLength(500);

        builder.HasOne(d => d.Customer)
            .WithMany(c => c.Deals)
            .HasForeignKey(d => d.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Stage)
            .WithMany(s => s.Deals)
            .HasForeignKey(d => d.StageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.CreatedByUser)
            .WithMany(u => u.CreatedDeals)
            .HasForeignKey(d => d.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.AssignedToUser)
            .WithMany(u => u.AssignedDeals)
            .HasForeignKey(d => d.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(d => d.CustomerId);
        builder.HasIndex(d => d.StageId);
        builder.HasIndex(d => d.AssignedToUserId);
        builder.HasIndex(d => d.ExpectedCloseDate);
    }
}
