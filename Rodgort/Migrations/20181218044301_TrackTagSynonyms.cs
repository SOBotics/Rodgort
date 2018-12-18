using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class TrackTagSynonyms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SynonymOfTagName",
                table: "Tags",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_SynonymOfTagName",
                table: "Tags",
                column: "SynonymOfTagName");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Tags_SynonymOfTagName",
                table: "Tags",
                column: "SynonymOfTagName",
                principalTable: "Tags",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Tags_SynonymOfTagName",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_SynonymOfTagName",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "SynonymOfTagName",
                table: "Tags");
        }
    }
}
