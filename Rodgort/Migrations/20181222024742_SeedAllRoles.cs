using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class SeedAllRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                column: "Name",
                value: "Moderator");

            migrationBuilder.InsertData(
                table: "Roles",
                column: "Name",
                value: "Rodgort Admin");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Name",
                keyValue: "Moderator");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Name",
                keyValue: "Rodgort Admin");
        }
    }
}
