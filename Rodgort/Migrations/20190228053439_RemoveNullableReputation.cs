using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class RemoveNullableReputation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "reputation",
                table: "site_users",
                nullable: false,
                oldClrType: typeof(int),
                oldDefaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "reputation",
                table: "site_users",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int));
        }
    }
}
