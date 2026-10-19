using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zamanla.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookingCancelHoursBefore",
                table: "Tenants",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<int>(
                name: "BookingMaxDaysAhead",
                table: "Tenants",
                type: "int",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.AddColumn<int>(
                name: "BookingMaxUpcomingPerPhone",
                table: "Tenants",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Tenants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Tenants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullAddress",
                table: "Tenants",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Tenants",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingCancelHoursBefore",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "BookingMaxDaysAhead",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "BookingMaxUpcomingPerPhone",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "FullAddress",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Tenants");
        }
    }
}
