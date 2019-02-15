using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class AddForeignKeyToMetaQuestionOnAudits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionTagTrackingStatusAudits_MetaQuestions_MetaQuest~",
                table: "MetaQuestionTagTrackingStatusAudits",
                column: "MetaQuestionId",
                principalTable: "MetaQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionTagTrackingStatusAudits_MetaQuestions_MetaQuest~",
                table: "MetaQuestionTagTrackingStatusAudits");
        }
    }
}
