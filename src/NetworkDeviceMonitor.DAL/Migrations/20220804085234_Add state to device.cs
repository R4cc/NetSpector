using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetworkDeviceMonitor.DAL.Migrations
{
    public partial class Addstatetodevice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                table: "Devices",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnline",
                table: "Devices");
        }
    }
}
