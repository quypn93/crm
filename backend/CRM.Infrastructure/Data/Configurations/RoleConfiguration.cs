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

        builder.HasData(
            new Role { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = RoleNames.Admin,             Description = "Quản trị viên hệ thống",         CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = RoleNames.SalesManager,      Description = "Quản lý kinh doanh",             CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = RoleNames.SalesRep,          Description = "Nhân viên kinh doanh",            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = RoleNames.ProductionManager, Description = "Quản lý sản xuất",               CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = RoleNames.ProductionStaff,   Description = "Nhân viên sản xuất",              CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = RoleNames.QualityManager,    Description = "Quản lý kiểm soát chất lượng",   CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = RoleNames.QualityControl,    Description = "Nhân viên kiểm soát chất lượng", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Name = RoleNames.DeliveryManager,   Description = "Quản lý giao hàng",              CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), Name = RoleNames.DeliveryStaff,     Description = "Nhân viên giao hàng",             CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = RoleNames.DesignManager,     Description = "Quản lý thiết kế",               CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = RoleNames.Designer,          Description = "Nhân viên thiết kế",              CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
