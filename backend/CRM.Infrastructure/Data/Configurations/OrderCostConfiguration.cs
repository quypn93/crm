using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class OrderCostConfiguration : IEntityTypeConfiguration<OrderCost>
{
    public void Configure(EntityTypeBuilder<OrderCost> builder)
    {
        builder.ToTable("OrderCosts");

        builder.HasKey(x => x.Id);

        // 1 đơn chỉ có 1 bản ghi chi phí.
        builder.HasIndex(x => x.OrderId).IsUnique();

        builder.Property(x => x.CostAmount).HasPrecision(18, 2);
        builder.Property(x => x.ShippingCost).HasPrecision(18, 2);
        builder.Property(x => x.OutboundShippingCost).HasPrecision(18, 2);
        builder.Property(x => x.OtherCost).HasPrecision(18, 2);
        builder.Property(x => x.TotalCost).HasPrecision(18, 2);

        builder.Property(x => x.CostFileUrl).HasMaxLength(500);
        builder.Property(x => x.CostFileName).HasMaxLength(255);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne(x => x.Order)
            .WithOne()
            .HasForeignKey<OrderCost>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.EnteredByUser)
            .WithMany()
            .HasForeignKey(x => x.EnteredByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
