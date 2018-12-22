using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class AddUnknownDeletionType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "UserActionTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 7, "Unknown deletion" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserActionTypes",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
