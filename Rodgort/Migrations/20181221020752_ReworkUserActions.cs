using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Rodgort.Migrations
{
    public partial class ReworkUserActions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRetags");

            migrationBuilder.CreateTable(
                name: "SiteUsers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    DisplayName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserActionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserActions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    SiteUserId = table.Column<int>(nullable: false),
                    PostId = table.Column<int>(nullable: false),
                    Tag = table.Column<string>(nullable: true),
                    Time = table.Column<DateTime>(nullable: false),
                    UserActionTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActions_SiteUsers_SiteUserId",
                        column: x => x.SiteUserId,
                        principalTable: "SiteUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserActions_UserActionTypes_UserActionTypeId",
                        column: x => x.UserActionTypeId,
                        principalTable: "UserActionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "UserActionTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Removed Tag" },
                    { 2, "Added Tag" },
                    { 3, "Closed" },
                    { 4, "Reopened" },
                    { 5, "Deleted" },
                    { 6, "Undeleted" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserActions_SiteUserId",
                table: "UserActions",
                column: "SiteUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActions_UserActionTypeId",
                table: "UserActions",
                column: "UserActionTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserActions");

            migrationBuilder.DropTable(
                name: "SiteUsers");

            migrationBuilder.DropTable(
                name: "UserActionTypes");

            migrationBuilder.CreateTable(
                name: "UserRetags",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Removed = table.Column<bool>(nullable: false),
                    Tag = table.Column<string>(nullable: true),
                    Time = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRetags", x => x.Id);
                });
        }
    }
}
