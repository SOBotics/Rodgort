using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class RemoveDatabaseDefaults : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeProcessed",
                table: "UserActions",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "now() at time zone 'utc'");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateAdded",
                table: "SiteUserRoles",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "now() at time zone 'utc'");

            migrationBuilder.AlterColumn<int>(
                name: "AddedByUserId",
                table: "SiteUserRoles",
                nullable: false,
                oldClrType: typeof(int),
                oldDefaultValue: -1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeProcessed",
                table: "UserActions",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'",
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateAdded",
                table: "SiteUserRoles",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'",
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<int>(
                name: "AddedByUserId",
                table: "SiteUserRoles",
                nullable: false,
                defaultValue: -1,
                oldClrType: typeof(int));
        }
    }
}
