using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class DealStageConfiguration : IEntityTypeConfiguration<DealStage>
{
    public void Configure(EntityTypeBuilder<DealStage> builder)
    {
        builder.ToTable("DealStages");

        builder.HasKey(ds => ds.Id);

        builder.Property(ds => ds.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ds => ds.Color)
            .HasMaxLength(20);

        // Seed data - Quy trình sản xuất đồng phục Đồng Phục Bốn Mùa
        builder.HasData(
            // 1. Tiềm năng - Khách hàng mới liên hệ, hỏi thông tin
            new DealStage
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Tiềm năng",
                Order = 1,
                Color = "#6366F1",
                Probability = 10,
                IsDefault = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // 2. Báo giá - Đã gửi báo giá cho khách
            new DealStage
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = "Báo giá",
                Order = 2,
                Color = "#8B5CF6",
                Probability = 25,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // 3. Duyệt mẫu - Khách đang xem xét mẫu thiết kế
            new DealStage
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Name = "Duyệt mẫu",
                Order = 3,
                Color = "#EC4899",
                Probability = 50,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // 4. Xác nhận đơn - Khách đã chốt đơn, đặt cọc
            new DealStage
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Name = "Xác nhận đơn",
                Order = 4,
                Color = "#F59E0B",
                Probability = 75,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // 5. Đang sản xuất - Đơn hàng đang được may
            new DealStage
            {
                Id = Guid.Parse("11111111-aaaa-bbbb-cccc-dddddddddddd"),
                Name = "Đang sản xuất",
                Order = 5,
                Color = "#14B8A6",
                Probability = 90,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // 6. Giao hàng - Đang vận chuyển đến khách
            new DealStage
            {
                Id = Guid.Parse("22222222-aaaa-bbbb-cccc-dddddddddddd"),
                Name = "Giao hàng",
                Order = 6,
                Color = "#0EA5E9",
                Probability = 95,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // 7. Hoàn thành - Đã giao hàng và thanh toán xong
            new DealStage
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                Name = "Hoàn thành",
                Order = 7,
                Color = "#10B981",
                Probability = 100,
                IsWonStage = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // 8. Đã hủy - Khách hủy đơn hoặc không chốt
            new DealStage
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                Name = "Đã hủy",
                Order = 8,
                Color = "#EF4444",
                Probability = 0,
                IsLostStage = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
