using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialIdToColorFabric : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MaterialId",
                table: "ColorFabrics",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ColorFabrics_MaterialId",
                table: "ColorFabrics",
                column: "MaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_ColorFabrics_Materials_MaterialId",
                table: "ColorFabrics",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ColorFabrics_Materials_MaterialId",
                table: "ColorFabrics");

            migrationBuilder.DropIndex(
                name: "IX_ColorFabrics_MaterialId",
                table: "ColorFabrics");

            migrationBuilder.DropColumn(
                name: "MaterialId",
                table: "ColorFabrics");
        }
    }
}
