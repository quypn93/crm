using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class ProductionStageConfiguration : IEntityTypeConfiguration<ProductionStage>
{
    public void Configure(EntityTypeBuilder<ProductionStage> builder)
    {
        builder.ToTable("ProductionStages");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.StageName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.ResponsibleRole)
            .HasMaxLength(50);

        builder.HasIndex(s => s.StageOrder);
        builder.HasIndex(s => s.IsActive);
    }
}
