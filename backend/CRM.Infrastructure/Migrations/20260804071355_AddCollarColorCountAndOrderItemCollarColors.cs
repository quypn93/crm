using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCollarColorCountAndOrderItemCollarColors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CollarColor1Id",
                table: "OrderItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollarColor1Name",
                table: "OrderItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CollarColor2Id",
                table: "OrderItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollarColor2Name",
                table: "OrderItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CollarColor3Id",
                table: "OrderItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollarColor3Name",
                table: "OrderItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ColorCount",
                table: "Collars",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Backfill ColorCount cho các bo cổ đã seed/nhập trước migration này (DataSeeder chỉ
            // seed khi bảng rỗng nên prod đã có data sẽ không tự có cột này) — theo cột
            // "QUY ĐỊNH CHỌN MÀU" của bảng bo cổ gốc: 2 = màu chính+phối, 3 = +phối 1&2.
            migrationBuilder.Sql(@"
                UPDATE ""Collars"" SET ""ColorCount"" = 2
                WHERE ""Name"" IN ('X-02','X-03','X-05','X-06','X-07','X-08','X-12','X-14');

                UPDATE ""Collars"" SET ""ColorCount"" = 3
                WHERE ""Name"" IN ('X-04','X-09','X-10','X-11','X-13','X-15');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollarColor1Id",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CollarColor1Name",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CollarColor2Id",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CollarColor2Name",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CollarColor3Id",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CollarColor3Name",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ColorCount",
                table: "Collars");
        }
    }
}
