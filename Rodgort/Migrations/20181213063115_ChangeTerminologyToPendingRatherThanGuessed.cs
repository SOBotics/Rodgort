using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class ChangeTerminologyToPendingRatherThanGuessed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MetaQuestionTagStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Pending");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MetaQuestionTagStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Guessed");
        }
    }
}
