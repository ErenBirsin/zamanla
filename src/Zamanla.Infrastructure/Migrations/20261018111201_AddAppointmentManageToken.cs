using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zamanla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentManageToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ManageToken",
                table: "Appointments",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ManageToken",
                table: "Appointments",
                column: "ManageToken",
                unique: true,
                filter: "[ManageToken] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_ManageToken",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ManageToken",
                table: "Appointments");
        }
    }
}
