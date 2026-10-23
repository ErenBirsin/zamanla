using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zamanla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddForgotPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ForgotPasswordCode",
                table: "AppUsers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ForgotPasswordDate",
                table: "AppUsers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsForgotPasswordCompleted",
                table: "AppUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_ForgotPasswordCode",
                table: "AppUsers",
                column: "ForgotPasswordCode",
                unique: true,
                filter: "[ForgotPasswordCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppUsers_ForgotPasswordCode",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "ForgotPasswordCode",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "ForgotPasswordDate",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "IsForgotPasswordCompleted",
                table: "AppUsers");
        }
    }
}
