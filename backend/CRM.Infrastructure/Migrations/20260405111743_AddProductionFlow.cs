using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DesignerUserId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrCodeImageBase64",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrCodeToken",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackImageUrl",
                table: "Designs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorText",
                table: "Designs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrontImageUrl",
                table: "Designs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GiftItems",
                table: "Designs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialText",
                table: "Designs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonNamesBySize",
                table: "Designs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnDate",
                table: "Designs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SizeQuantities",
                table: "Designs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StyleText",
                table: "Designs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductionStages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StageOrder = table.Column<int>(type: "int", nullable: false),
                    StageName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ResponsibleRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionStages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderProductionSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductionStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderProductionSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderProductionSteps_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderProductionSteps_ProductionStages_ProductionStageId",
                        column: x => x.ProductionStageId,
                        principalTable: "ProductionStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderProductionSteps_Users_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DesignerUserId",
                table: "Orders",
                column: "DesignerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProductionSteps_CompletedByUserId",
                table: "OrderProductionSteps",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProductionSteps_IsCompleted",
                table: "OrderProductionSteps",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProductionSteps_OrderId",
                table: "OrderProductionSteps",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProductionSteps_OrderId_ProductionStageId",
                table: "OrderProductionSteps",
                columns: new[] { "OrderId", "ProductionStageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderProductionSteps_ProductionStageId",
                table: "OrderProductionSteps",
                column: "ProductionStageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStages_IsActive",
                table: "ProductionStages",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStages_StageOrder",
                table: "ProductionStages",
                column: "StageOrder");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_DesignerUserId",
                table: "Orders",
                column: "DesignerUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_DesignerUserId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "OrderProductionSteps");

            migrationBuilder.DropTable(
                name: "ProductionStages");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DesignerUserId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DesignerUserId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "QrCodeImageBase64",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "QrCodeToken",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BackImageUrl",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "ColorText",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "FrontImageUrl",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "GiftItems",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "MaterialText",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "PersonNamesBySize",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "ReturnDate",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "SizeQuantities",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "StyleText",
                table: "Designs");
        }
    }
}
