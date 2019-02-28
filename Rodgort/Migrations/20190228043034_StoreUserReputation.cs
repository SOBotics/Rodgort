using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class StoreUserReputation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "reputation",
                table: "site_users",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reputation",
                table: "site_users");
        }
    }
}
