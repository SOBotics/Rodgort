using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class RemoveDefaultValueForIsSynonym : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "is_synonym",
                table: "tag_statistics",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "is_synonym",
                table: "tag_statistics",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool));
        }
    }
}
