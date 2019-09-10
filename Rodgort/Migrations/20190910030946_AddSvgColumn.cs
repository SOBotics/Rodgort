using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class AddSvgColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "progress_svg",
                table: "meta_questions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "progress_svg",
                table: "meta_questions");
        }
    }
}
