using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetworkDeviceMonitor.DAL.Migrations
{
    public partial class addexclusions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exclusion",
                columns: table => new
                {
                    ExclusionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartIpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    EndIpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    NetworkId = table.Column<int>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exclusion", x => x.ExclusionId);
                    table.ForeignKey(
                        name: "FK_Exclusion_Networks_NetworkId",
                        column: x => x.NetworkId,
                        principalTable: "Networks",
                        principalColumn: "NetworkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exclusion_NetworkId",
                table: "Exclusion",
                column: "NetworkId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exclusion");
        }
    }
}
