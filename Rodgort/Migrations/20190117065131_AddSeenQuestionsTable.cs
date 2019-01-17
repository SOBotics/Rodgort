using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class AddSeenQuestionsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeenQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Tag = table.Column<string>(nullable: false),
                    LastSeen = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeenQuestions", x => new { x.Id, x.Tag });
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeenQuestions_Tag",
                table: "SeenQuestions",
                column: "Tag");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeenQuestions");
        }
    }
}
