using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class TaskNotificationLogConfiguration : IEntityTypeConfiguration<TaskNotificationLog>
{
    public void Configure(EntityTypeBuilder<TaskNotificationLog> builder)
    {
        builder.ToTable("TaskNotificationLogs");

        builder.HasKey(l => l.Id);

        builder.HasOne(l => l.Task)
            .WithMany()
            .HasForeignKey(l => l.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => new { l.TaskId, l.Type })
            .IsUnique();
    }
}
