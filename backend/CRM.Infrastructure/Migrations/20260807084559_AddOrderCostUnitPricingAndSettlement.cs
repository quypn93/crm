using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCostUnitPricingAndSettlement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GiftAmount",
                table: "OrderCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "GiftQuantity",
                table: "OrderCosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "GiftUnitCost",
                table: "OrderCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SettlementAmount",
                table: "OrderCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCode",
                table: "OrderCosts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalQuantity",
                table: "OrderCosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "OrderCosts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiftAmount",
                table: "OrderCosts");

            migrationBuilder.DropColumn(
                name: "GiftQuantity",
                table: "OrderCosts");

            migrationBuilder.DropColumn(
                name: "GiftUnitCost",
                table: "OrderCosts");

            migrationBuilder.DropColumn(
                name: "SettlementAmount",
                table: "OrderCosts");

            migrationBuilder.DropColumn(
                name: "ShippingCode",
                table: "OrderCosts");

            migrationBuilder.DropColumn(
                name: "TotalQuantity",
                table: "OrderCosts");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "OrderCosts");
        }
    }
}
