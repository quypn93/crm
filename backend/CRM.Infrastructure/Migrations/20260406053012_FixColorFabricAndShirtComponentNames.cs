using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixColorFabricAndShirtComponentNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix ColorFabrics — thêm dấu tiếng Việt
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = N'Trắng',         Description = N'Màu trắng cơ bản'       WHERE Name = 'Trang'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = N'Đen',           Description = N'Màu đen cơ bản'         WHERE Name = 'Den'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = N'Xanh Dương',    Description = N'Màu xanh dương đậm'     WHERE Name = 'Xanh Duong'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = N'Xanh Dương Nhạt', Description = N'Màu xanh dương nhạt' WHERE Name = 'Xanh Duong Nhat'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = N'Xanh Lá',       Description = N'Màu xanh lá cây'        WHERE Name = 'Xanh La'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = N'Đỏ',            Description = N'Màu đỏ tươi'            WHERE Name = 'Do'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = N'Vàng',          Description = N'Màu vàng'               WHERE Name = 'Vang'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = N'Cam',           Description = N'Màu cam'                WHERE Name = 'Cam'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = N'Tím',           Description = N'Màu tím'                WHERE Name = 'Tim'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = N'Hồng',          Description = N'Màu hồng'               WHERE Name = 'Hong'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = N'Xám',           Description = N'Màu xám'                WHERE Name = 'Xam'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = N'Nâu',           Description = N'Màu nâu'                WHERE Name = 'Nau'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = N'Be/Kem',        Description = N'Màu be/kem'             WHERE Name = 'Be'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = N'Xanh Rêu',      Description = N'Màu xanh rêu'           WHERE Name = 'Xanh Reu'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = N'Đỏ Đô',         Description = N'Màu đỏ đô/burgundy'     WHERE Name = 'Do Do'");

            // Fix ShirtComponents — thêm dấu tiếng Việt
            // Collar
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Cổ Tròn'    WHERE Name = 'Co Tron'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Cổ Đức'     WHERE Name = 'Co Duc'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Cổ Trụ'     WHERE Name = 'Co Tru'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Cổ Bầu'     WHERE Name = 'Co Bau'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Cổ Tim'     WHERE Name = 'Co Tim'");
            // Sleeve
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Tay Ngắn'   WHERE Name = 'Tay Ngan'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Tay Dài'    WHERE Name = 'Tay Dai'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Tay Raglan' WHERE Name = 'Tay Raglan'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Tay Phồng'  WHERE Name = 'Tay Phong'");
            // Button
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Nút Trắng'  WHERE Name = 'Nut Trang'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Nút Đen'    WHERE Name = 'Nut Den'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Nút Xanh'   WHERE Name = 'Nut Xanh'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Nút Kim Loại' WHERE Name = 'Nut Kim Loai'");
            // Fabric
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Cotton Pha' WHERE Name = 'Cotton Pha'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Thun Cá Sấu' WHERE Name = 'Thun Ca Sau'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Thun Cotton' WHERE Name = 'Thun Cotton'");
            // Body
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Ôm (Slim Fit)'     WHERE Name = 'Om (Slim Fit)'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Vừa (Regular Fit)' WHERE Name = 'Vua (Regular Fit)'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Rộng (Loose Fit)'  WHERE Name = 'Rong (Loose Fit)'");
            // Stripe
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Không Sọc'  WHERE Name = 'Khong Soc'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Sọc Dọc'    WHERE Name = 'Soc Doc'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Sọc Ngang'  WHERE Name = 'Soc Ngang'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Sọc Chéo'   WHERE Name = 'Soc Cheo'");
            // Collar Stripe
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Viền Cổ Đơn'  WHERE Name = 'Vien Co Don'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Viền Cổ Đôi'  WHERE Name = 'Vien Co Doi'");
            migrationBuilder.Sql("UPDATE ShirtComponents SET Name = N'Không Viền Cổ' WHERE Name = 'Khong Vien Co'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert ColorFabrics
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = 'Trang',          Description = 'Mau trang co ban'        WHERE Name = N'Trắng'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = 'Den',            Description = 'Mau den co ban'          WHERE Name = N'Đen'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = 'Xanh Duong',     Description = 'Mau xanh duong dam'      WHERE Name = N'Xanh Dương'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = 'Xanh Duong Nhat',Description = 'Mau xanh duong nhat'     WHERE Name = N'Xanh Dương Nhạt'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = 'Xanh La',        Description = 'Mau xanh la cay'         WHERE Name = N'Xanh Lá'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = 'Do',             Description = 'Mau do tuoi'             WHERE Name = N'Đỏ'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = 'Vang',           Description = 'Mau vang'                WHERE Name = N'Vàng'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = 'Cam',            Description = 'Mau cam'                 WHERE Name = N'Cam'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = 'Tim',            Description = 'Mau tim'                 WHERE Name = N'Tím'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = 'Hong',           Description = 'Mau hong'                WHERE Name = N'Hồng'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = 'Xam',            Description = 'Mau xam'                 WHERE Name = N'Xám'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = 'Nau',            Description = 'Mau nau'                 WHERE Name = N'Nâu'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = 'Be',             Description = 'Mau be/kem'              WHERE Name = N'Be/Kem'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = 'Xanh Reu',       Description = 'Mau xanh reu'            WHERE Name = N'Xanh Rêu'");
            migrationBuilder.Sql("UPDATE ColorFabrics SET Name = 'Do Do',          Description = 'Mau do do/burgundy'      WHERE Name = N'Đỏ Đô'");
        }
    }
}
