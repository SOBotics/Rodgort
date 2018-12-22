using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class BurnakiFollowsRequireATag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tag",
                table: "BurnakiFollows",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tag",
                table: "BurnakiFollows");
        }
    }
}
