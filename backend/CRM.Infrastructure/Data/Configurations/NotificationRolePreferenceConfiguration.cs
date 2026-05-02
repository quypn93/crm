using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class NotificationRolePreferenceConfiguration : IEntityTypeConfiguration<NotificationRolePreference>
{
    public void Configure(EntityTypeBuilder<NotificationRolePreference> builder)
    {
        builder.ToTable("NotificationRolePreferences");

        builder.HasKey(p => p.Id);

        builder.HasOne(p => p.Role)
            .WithMany()
            .HasForeignKey(p => p.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.RoleId, p.Type })
            .IsUnique();
    }
}
