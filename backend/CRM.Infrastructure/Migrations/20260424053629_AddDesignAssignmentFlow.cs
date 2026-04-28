using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDesignAssignmentFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DesignId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AccentColorFabricId",
                table: "Designs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedToUserId",
                table: "Designs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignmentNotes",
                table: "Designs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackLogoUrl",
                table: "Designs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChestLogoUrl",
                table: "Designs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Designs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletedImageUrl",
                table: "Designs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShirtFormId",
                table: "Designs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Designs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DesignId",
                table: "Orders",
                column: "DesignId");

            migrationBuilder.CreateIndex(
                name: "IX_Designs_AccentColorFabricId",
                table: "Designs",
                column: "AccentColorFabricId");

            migrationBuilder.CreateIndex(
                name: "IX_Designs_AssignedToUserId",
                table: "Designs",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Designs_ShirtFormId",
                table: "Designs",
                column: "ShirtFormId");

            migrationBuilder.CreateIndex(
                name: "IX_Designs_Status",
                table: "Designs",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Designs_ColorFabrics_AccentColorFabricId",
                table: "Designs",
                column: "AccentColorFabricId",
                principalTable: "ColorFabrics",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Designs_ProductForms_ShirtFormId",
                table: "Designs",
                column: "ShirtFormId",
                principalTable: "ProductForms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Designs_Users_AssignedToUserId",
                table: "Designs",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Designs_DesignId",
                table: "Orders",
                column: "DesignId",
                principalTable: "Designs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Designs_ColorFabrics_AccentColorFabricId",
                table: "Designs");

            migrationBuilder.DropForeignKey(
                name: "FK_Designs_ProductForms_ShirtFormId",
                table: "Designs");

            migrationBuilder.DropForeignKey(
                name: "FK_Designs_Users_AssignedToUserId",
                table: "Designs");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Designs_DesignId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DesignId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Designs_AccentColorFabricId",
                table: "Designs");

            migrationBuilder.DropIndex(
                name: "IX_Designs_AssignedToUserId",
                table: "Designs");

            migrationBuilder.DropIndex(
                name: "IX_Designs_ShirtFormId",
                table: "Designs");

            migrationBuilder.DropIndex(
                name: "IX_Designs_Status",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "DesignId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AccentColorFabricId",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "AssignmentNotes",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "BackLogoUrl",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "ChestLogoUrl",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "CompletedImageUrl",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "ShirtFormId",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Designs");
        }
    }
}
