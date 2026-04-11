using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.ProductName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(oi => oi.ProductCode)
            .HasMaxLength(50);

        builder.Property(oi => oi.Description)
            .HasMaxLength(1000);

        builder.Property(oi => oi.Size)
            .HasMaxLength(50);

        builder.Property(oi => oi.Color)
            .HasMaxLength(50);

        builder.Property(oi => oi.Material)
            .HasMaxLength(100);

        builder.Property(oi => oi.Unit)
            .HasMaxLength(20)
            .HasDefaultValue("cái");

        builder.Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(oi => oi.DiscountPercent)
            .HasPrecision(5, 2);

        builder.Property(oi => oi.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(oi => oi.LineTotal)
            .HasPrecision(18, 2);

        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(oi => oi.OrderId);
    }
}
