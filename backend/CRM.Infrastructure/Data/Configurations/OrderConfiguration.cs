using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("VND");

        builder.Property(o => o.SubTotal)
            .HasPrecision(18, 2);

        builder.Property(o => o.DiscountPercent)
            .HasPrecision(5, 2);

        builder.Property(o => o.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.TaxPercent)
            .HasPrecision(5, 2);

        builder.Property(o => o.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.PaidAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.ShippingAddress)
            .HasMaxLength(500);

        builder.Property(o => o.ShippingCity)
            .HasMaxLength(100);

        builder.Property(o => o.ShippingPhone)
            .HasMaxLength(50);

        builder.Property(o => o.PaymentMethod)
            .HasMaxLength(100);

        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Deal)
            .WithMany(d => d.Orders)
            .HasForeignKey(o => o.DealId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.CreatedByUser)
            .WithMany(u => u.CreatedOrders)
            .HasForeignKey(o => o.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.AssignedToUser)
            .WithMany(u => u.AssignedOrders)
            .HasForeignKey(o => o.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.DealId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.PaymentStatus);
        builder.HasIndex(o => o.OrderDate);
    }
}
