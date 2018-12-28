using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class RenameNoneToRequiresTrackingApproval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MetaQuestionTagStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Requires Tracking Approval");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MetaQuestionTagStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "None");
        }
    }
}
