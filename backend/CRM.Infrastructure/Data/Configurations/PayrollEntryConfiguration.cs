using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class PayrollEntryConfiguration : IEntityTypeConfiguration<PayrollEntry>
{
    public void Configure(EntityTypeBuilder<PayrollEntry> builder)
    {
        builder.ToTable("PayrollEntries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmployeeName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Position).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.Property(x => x.Salary).HasPrecision(18, 2);
        builder.Property(x => x.Allowance).HasPrecision(18, 2);
        builder.Property(x => x.Insurance).HasPrecision(18, 2);
        builder.Property(x => x.OtherCost).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);

        // Truy vấn luôn theo kỳ lương.
        builder.HasIndex(x => new { x.Year, x.Month });

        // 1 nhân sự có tài khoản chỉ 1 dòng lương/kỳ. Dòng nhập tay (UserId null) không chặn.
        builder.HasIndex(x => new { x.Year, x.Month, x.UserId })
            .IsUnique()
            .HasFilter("\"UserId\" IS NOT NULL");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
