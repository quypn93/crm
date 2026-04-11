using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class OrderProductionStepConfiguration : IEntityTypeConfiguration<OrderProductionStep>
{
    public void Configure(EntityTypeBuilder<OrderProductionStep> builder)
    {
        builder.ToTable("OrderProductionSteps");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Notes)
            .HasMaxLength(1000);

        builder.HasOne(s => s.Order)
            .WithMany(o => o.ProductionSteps)
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ProductionStage)
            .WithMany(ps => ps.Steps)
            .HasForeignKey(s => s.ProductionStageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.CompletedByUser)
            .WithMany()
            .HasForeignKey(s => s.CompletedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Mỗi đơn chỉ có 1 record per stage
        builder.HasIndex(s => new { s.OrderId, s.ProductionStageId }).IsUnique();
        builder.HasIndex(s => s.IsCompleted);
        builder.HasIndex(s => s.OrderId);
    }
}
