using CRM.Core.Entities;
using CRM.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.Priority)
            .HasDefaultValue(TaskPriority.Medium)
            .HasSentinel(TaskPriority.Medium);

        builder.Property(t => t.Status)
            .HasDefaultValue(CRM.Core.Enums.TaskStatus.Pending)
            .HasSentinel(CRM.Core.Enums.TaskStatus.Pending);

        builder.HasOne(t => t.Customer)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Deal)
            .WithMany(d => d.Tasks)
            .HasForeignKey(t => t.DealId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.CreatedByUser)
            .WithMany(u => u.CreatedTasks)
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedToUser)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => t.CustomerId);
        builder.HasIndex(t => t.DealId);
        builder.HasIndex(t => t.AssignedToUserId);
        builder.HasIndex(t => t.DueDate);
        builder.HasIndex(t => t.Status);
    }
}
