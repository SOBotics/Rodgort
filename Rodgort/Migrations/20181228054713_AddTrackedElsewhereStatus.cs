using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class AddTrackedElsewhereStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MetaQuestionTagStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Requires tracking approval");

            migrationBuilder.InsertData(
                table: "MetaQuestionTagStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[] { 4, "Tracked elsewhere" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MetaQuestionTagStatuses",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "MetaQuestionTagStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Requires Tracking Approval");
        }
    }
}
