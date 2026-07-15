using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedUserIdToSenderAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedUserId",
                table: "SenderAddresses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SenderAddresses_AssignedUserId",
                table: "SenderAddresses",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SenderAddresses_Users_AssignedUserId",
                table: "SenderAddresses",
                column: "AssignedUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SenderAddresses_Users_AssignedUserId",
                table: "SenderAddresses");

            migrationBuilder.DropIndex(
                name: "IX_SenderAddresses_AssignedUserId",
                table: "SenderAddresses");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "SenderAddresses");
        }
    }
}
