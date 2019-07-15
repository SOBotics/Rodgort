using Microsoft.EntityFrameworkCore.Migrations;
using Rodgort.Data.Tables;

namespace Rodgort.Migrations
{
    public partial class FixInvalidTagTrackingStatusAudits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
UPDATE meta_question_tag_tracking_status_audits
SET previous_tracking_status_id = null
where previous_tracking_status_id = new_tracking_status_id
and previous_tracking_status_id = {DbMetaQuestionTagTrackingStatus.REQUIRES_TRACKING_APPROVAL}
");

            migrationBuilder.Sql(@"
DELETE FROM meta_question_tag_tracking_status_audits
where previous_tracking_status_id = new_tracking_status_id
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
