using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShirtDesignFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ColorFabrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorFabrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Designs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DesignName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DesignData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SelectedComponents = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Designer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustomerFullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Total = table.Column<int>(type: "int", nullable: true),
                    SizeMan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SizeWomen = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SizeKid = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Oversized = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FinishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NoteConfection = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoteOldCodeOrder = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NoteAttachTagLabel = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NoteOther = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaleStaff = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ColorFabricId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Designs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Designs_ColorFabrics_ColorFabricId",
                        column: x => x.ColorFabricId,
                        principalTable: "ColorFabrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Designs_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Designs_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ShirtComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WomenImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ColorFabricId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShirtComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShirtComponents_ColorFabrics_ColorFabricId",
                        column: x => x.ColorFabricId,
                        principalTable: "ColorFabrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ColorFabrics_Name",
                table: "ColorFabrics",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Designs_ColorFabricId",
                table: "Designs",
                column: "ColorFabricId");

            migrationBuilder.CreateIndex(
                name: "IX_Designs_CreatedAt",
                table: "Designs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Designs_CreatedByUserId",
                table: "Designs",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Designs_OrderId",
                table: "Designs",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ShirtComponents_ColorFabricId",
                table: "ShirtComponents",
                column: "ColorFabricId");

            migrationBuilder.CreateIndex(
                name: "IX_ShirtComponents_IsDeleted",
                table: "ShirtComponents",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ShirtComponents_Type",
                table: "ShirtComponents",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Designs");

            migrationBuilder.DropTable(
                name: "ShirtComponents");

            migrationBuilder.DropTable(
                name: "ColorFabrics");
        }
    }
}
