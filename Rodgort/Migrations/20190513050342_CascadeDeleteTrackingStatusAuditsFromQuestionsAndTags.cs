using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class CascadeDeleteTrackingStatusAuditsFromQuestionsAndTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_meta_question_tag_tracking_status_audits_meta_question_tags_meta_q~",
                table: "meta_question_tag_tracking_status_audits");

            migrationBuilder.AddForeignKey(
                name: "fk_meta_question_tag_tracking_status_audits_meta_question_tags_meta_q~",
                table: "meta_question_tag_tracking_status_audits",
                columns: new[] { "meta_question_id", "tag" },
                principalTable: "meta_question_tags",
                principalColumns: new[] { "meta_question_id", "tag_name" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_meta_question_tag_tracking_status_audits_meta_question_tags_meta_q~",
                table: "meta_question_tag_tracking_status_audits");

            migrationBuilder.AddForeignKey(
                name: "fk_meta_question_tag_tracking_status_audits_meta_question_tags_meta_q~",
                table: "meta_question_tag_tracking_status_audits",
                columns: new[] { "meta_question_id", "tag" },
                principalTable: "meta_question_tags",
                principalColumns: new[] { "meta_question_id", "tag_name" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
