using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class TrackPhaseChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BurnEnded",
                table: "MetaQuestions",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BurnStarted",
                table: "MetaQuestions",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FeaturedEnded",
                table: "MetaQuestions",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FeaturedStarted",
                table: "MetaQuestions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BurnEnded",
                table: "MetaQuestions");

            migrationBuilder.DropColumn(
                name: "BurnStarted",
                table: "MetaQuestions");

            migrationBuilder.DropColumn(
                name: "FeaturedEnded",
                table: "MetaQuestions");

            migrationBuilder.DropColumn(
                name: "FeaturedStarted",
                table: "MetaQuestions");
        }
    }
}
