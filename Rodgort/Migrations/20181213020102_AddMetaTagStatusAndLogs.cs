using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Rodgort.Migrations
{
    public partial class AddMetaTagStatusAndLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "MetaQuestionTags",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    TimeLogged = table.Column<DateTime>(nullable: false),
                    Level = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Exception = table.Column<string>(nullable: true),
                    Logger = table.Column<string>(nullable: true),
                    Callsite = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetaQuestionTagStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaQuestionTagStatuses", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "MetaQuestionTagStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Guessed" });

            migrationBuilder.InsertData(
                table: "MetaQuestionTagStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[] { 2, "Approved" });

            migrationBuilder.InsertData(
                table: "MetaQuestionTagStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[] { 3, "Declined" });

            migrationBuilder.CreateIndex(
                name: "IX_MetaQuestionTags_StatusId",
                table: "MetaQuestionTags",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionTags_MetaQuestionTagStatuses_StatusId",
                table: "MetaQuestionTags",
                column: "StatusId",
                principalTable: "MetaQuestionTagStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionTags_MetaQuestionTagStatuses_StatusId",
                table: "MetaQuestionTags");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "MetaQuestionTagStatuses");

            migrationBuilder.DropIndex(
                name: "IX_MetaQuestionTags_StatusId",
                table: "MetaQuestionTags");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "MetaQuestionTags");
        }
    }
}
