using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGhtkTrackingToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GhtkFee",
                table: "Orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GhtkInsuranceFee",
                table: "Orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhtkLabel",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhtkLastError",
                table: "Orders",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhtkStatus",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GhtkStatusCode",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GhtkSyncedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhtkTrackingUrl",
                table: "Orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_GhtkLabel",
                table: "Orders",
                column: "GhtkLabel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_GhtkLabel",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GhtkFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GhtkInsuranceFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GhtkLabel",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GhtkLastError",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GhtkStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GhtkStatusCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GhtkSyncedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GhtkTrackingUrl",
                table: "Orders");
        }
    }
}
