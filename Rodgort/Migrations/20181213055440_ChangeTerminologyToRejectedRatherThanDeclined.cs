using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class ChangeTerminologyToRejectedRatherThanDeclined : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MetaQuestionTagStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Rejected");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MetaQuestionTagStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Declined");
        }
    }
}
