using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zamanla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffBookingVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookingVersion",
                table: "Staffs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingVersion",
                table: "Staffs");
        }
    }
}
