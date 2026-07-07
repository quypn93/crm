using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddViettelPostTrackingToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ViettelPostFee",
                table: "Orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ViettelPostInsuranceFee",
                table: "Orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ViettelPostLabel",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ViettelPostLastError",
                table: "Orders",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ViettelPostStatus",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViettelPostStatusCode",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ViettelPostSyncedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ViettelPostTrackingUrl",
                table: "Orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ViettelPostLabel",
                table: "Orders",
                column: "ViettelPostLabel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_ViettelPostLabel",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ViettelPostFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ViettelPostInsuranceFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ViettelPostLabel",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ViettelPostLastError",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ViettelPostStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ViettelPostStatusCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ViettelPostSyncedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ViettelPostTrackingUrl",
                table: "Orders");
        }
    }
}
