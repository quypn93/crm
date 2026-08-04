using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class FixedExpenseConfiguration : IEntityTypeConfiguration<FixedExpense>
{
    public void Configure(EntityTypeBuilder<FixedExpense> builder)
    {
        builder.ToTable("FixedExpenses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.CategoryName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property(x => x.AttachmentUrl).HasMaxLength(500);
        builder.Property(x => x.AttachmentName).HasMaxLength(255);

        builder.HasIndex(x => x.ExpenseDate);
        builder.HasIndex(x => new { x.ExpenseCategoryId, x.ExpenseDate });

        builder.HasOne(x => x.ExpenseCategory)
            .WithMany(c => c.Expenses)
            .HasForeignKey(x => x.ExpenseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);   // đầu mục đã phát sinh chi phí thì không cho xóa

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
