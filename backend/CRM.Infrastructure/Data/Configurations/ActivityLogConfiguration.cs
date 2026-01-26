using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        builder.HasOne(a => a.User)
            .WithMany(u => u.ActivityLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.CreatedAt);
    }
}
