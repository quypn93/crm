using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketingRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("15151515-1515-1515-1515-151515151515"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Marketing manager", "MarketingManager", null },
                    { new Guid("16161616-1616-1616-1616-161616161616"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Media marketing", "MediaMarketing", null },
                    { new Guid("17171717-1717-1717-1717-171717171717"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Digital ads", "DigitalAds", null },
                    { new Guid("18181818-1818-1818-1818-181818181818"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Media", "Media", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("16161616-1616-1616-1616-161616161616"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("17171717-1717-1717-1717-171717171717"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("18181818-1818-1818-1818-181818181818"));
        }
    }
}
