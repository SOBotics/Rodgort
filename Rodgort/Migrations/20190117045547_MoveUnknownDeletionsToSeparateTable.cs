using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Rodgort.Migrations
{
    public partial class MoveUnknownDeletionsToSeparateTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserActionTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.AddColumn<int>(
                name: "UnknownDeletionId",
                table: "UserActions",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DbUnknownDeletions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Time = table.Column<DateTime>(nullable: false),
                    Processed = table.Column<DateTime>(nullable: true),
                    PostId = table.Column<int>(nullable: false),
                    Tag = table.Column<string>(nullable: true),
                    ProcessedByUserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbUnknownDeletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DbUnknownDeletions_SiteUsers_ProcessedByUserId",
                        column: x => x.ProcessedByUserId,
                        principalTable: "SiteUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserActions_UnknownDeletionId",
                table: "UserActions",
                column: "UnknownDeletionId");

            migrationBuilder.CreateIndex(
                name: "IX_DbUnknownDeletions_ProcessedByUserId",
                table: "DbUnknownDeletions",
                column: "ProcessedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActions_DbUnknownDeletions_UnknownDeletionId",
                table: "UserActions",
                column: "UnknownDeletionId",
                principalTable: "DbUnknownDeletions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserActions_DbUnknownDeletions_UnknownDeletionId",
                table: "UserActions");

            migrationBuilder.DropTable(
                name: "DbUnknownDeletions");

            migrationBuilder.DropIndex(
                name: "IX_UserActions_UnknownDeletionId",
                table: "UserActions");

            migrationBuilder.DropColumn(
                name: "UnknownDeletionId",
                table: "UserActions");

            migrationBuilder.InsertData(
                table: "UserActionTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 7, "Unknown deletion" });
        }
    }
}
