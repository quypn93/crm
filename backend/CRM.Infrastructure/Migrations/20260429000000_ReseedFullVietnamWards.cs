using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReseedFullVietnamWards : Migration
    {
        // Xoá toàn bộ dữ liệu Provinces/Wards cũ để DataSeeder nạp lại từ JSON
        // mới (34 tỉnh + 3.321 phường/xã/đặc khu sau Nghị quyết sáp nhập 2025).
        // Order.ShippingProvinceCode/ShippingWardCode chỉ là string, không có FK,
        // nên các đơn hàng cũ vẫn còn nguyên giá trị denormalized (tên đã lưu).
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Xoá Wards trước (FK trỏ về Provinces) — dù cascade cũng dọn được.
            migrationBuilder.Sql(@"DELETE FROM ""Wards"";");
            migrationBuilder.Sql(@"DELETE FROM ""Provinces"";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Không có Down có ý nghĩa — dữ liệu cũ là sample không thể khôi phục.
            // Để rollback, chạy lại seeder sẽ nạp dữ liệu hiện tại từ JSON.
        }
    }
}
