using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(c => c.Name);

        builder.Property(c => c.Email)
            .HasMaxLength(255);

        builder.HasIndex(c => c.Email);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.Address)
            .HasMaxLength(500);

        builder.Property(c => c.City)
            .HasMaxLength(100);

        builder.Property(c => c.Country)
            .HasMaxLength(100);

        builder.Property(c => c.PostalCode)
            .HasMaxLength(20);

        builder.Property(c => c.CompanyName)
            .HasMaxLength(255);

        builder.HasIndex(c => c.CompanyName);

        builder.Property(c => c.Industry)
            .HasMaxLength(100);

        builder.Property(c => c.Website)
            .HasMaxLength(255);

        builder.HasOne(c => c.CreatedByUser)
            .WithMany(u => u.CreatedCustomers)
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.AssignedToUser)
            .WithMany(u => u.AssignedCustomers)
            .HasForeignKey(c => c.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.AssignedToUserId);
    }
}
