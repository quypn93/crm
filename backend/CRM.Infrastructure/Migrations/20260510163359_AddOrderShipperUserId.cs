using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderShipperUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_DesignerUserId",
                table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "ShipperUserId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShipperUserId",
                table: "Orders",
                column: "ShipperUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_DesignerUserId",
                table: "Orders",
                column: "DesignerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_ShipperUserId",
                table: "Orders",
                column: "ShipperUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_DesignerUserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_ShipperUserId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShipperUserId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShipperUserId",
                table: "Orders");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_DesignerUserId",
                table: "Orders",
                column: "DesignerUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
