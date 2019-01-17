using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class LogWhenUserActionWasProcessed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TimeProcessed",
                table: "UserActions",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeProcessed",
                table: "UserActions");
        }
    }
}
