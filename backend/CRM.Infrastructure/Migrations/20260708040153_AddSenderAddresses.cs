using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSenderAddresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SenderAddressId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SenderAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ProvinceId = table.Column<int>(type: "integer", nullable: false),
                    DistrictId = table.Column<int>(type: "integer", nullable: false),
                    WardId = table.Column<int>(type: "integer", nullable: false),
                    ProvinceName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DistrictName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    WardName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SenderAddresses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SenderAddressId",
                table: "Orders",
                column: "SenderAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_SenderAddresses_SenderAddressId",
                table: "Orders",
                column: "SenderAddressId",
                principalTable: "SenderAddresses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_SenderAddresses_SenderAddressId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "SenderAddresses");

            migrationBuilder.DropIndex(
                name: "IX_Orders_SenderAddressId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SenderAddressId",
                table: "Orders");
        }
    }
}
