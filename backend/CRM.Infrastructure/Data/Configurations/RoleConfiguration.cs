using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder.Property(r => r.Description)
            .HasMaxLength(255);

        // Seed data
        builder.HasData(
            new Role
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = RoleNames.Admin,
                Description = "Quản trị viên hệ thống với toàn quyền truy cập",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = RoleNames.SalesManager,
                Description = "Quản lý bán hàng với quyền quản lý đội ngũ",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = RoleNames.SalesRep,
                Description = "Nhân viên bán hàng với quyền truy cập tiêu chuẩn",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
