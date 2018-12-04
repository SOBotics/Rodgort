using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Rodgort.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DbMetaQuestion",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Link = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbMetaQuestion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DbRequestType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbRequestType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DbTag",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbTag", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "DbMetaAnswer",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    MetaQuestionId = table.Column<int>(nullable: false),
                    Body = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbMetaAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DbMetaAnswer_DbMetaQuestion_MetaQuestionId",
                        column: x => x.MetaQuestionId,
                        principalTable: "DbMetaQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DbMetaQuestionStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    MetaQuestionId = table.Column<int>(nullable: false),
                    Score = table.Column<int>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbMetaQuestionStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DbMetaQuestionStatistics_DbMetaQuestion_MetaQuestionId",
                        column: x => x.MetaQuestionId,
                        principalTable: "DbMetaQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DbMetaQuestionTag",
                columns: table => new
                {
                    MetaQuestionId = table.Column<int>(nullable: false),
                    TagName = table.Column<string>(nullable: false),
                    RequestTypeId = table.Column<int>(nullable: false),
                    SecondaryTagName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbMetaQuestionTag", x => new { x.MetaQuestionId, x.TagName });
                    table.ForeignKey(
                        name: "FK_DbMetaQuestionTag_DbMetaQuestion_MetaQuestionId",
                        column: x => x.MetaQuestionId,
                        principalTable: "DbMetaQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DbMetaQuestionTag_DbRequestType_RequestTypeId",
                        column: x => x.RequestTypeId,
                        principalTable: "DbRequestType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DbMetaQuestionTag_DbTag_SecondaryTagName",
                        column: x => x.SecondaryTagName,
                        principalTable: "DbTag",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DbMetaQuestionTag_DbTag_TagName",
                        column: x => x.TagName,
                        principalTable: "DbTag",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DbTagStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    TagName = table.Column<string>(nullable: true),
                    QuestionCount = table.Column<int>(nullable: false),
                    AnswerCount = table.Column<int>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbTagStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DbTagStatistics_DbTag_TagName",
                        column: x => x.TagName,
                        principalTable: "DbTag",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DbMetaAnswerStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    MetaAnswerId = table.Column<int>(nullable: false),
                    Score = table.Column<int>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbMetaAnswerStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DbMetaAnswerStatistics_DbMetaAnswer_MetaAnswerId",
                        column: x => x.MetaAnswerId,
                        principalTable: "DbMetaAnswer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DbRequestType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "Unknown" },
                    { 1, "Synonym" },
                    { 2, "Merge" },
                    { 3, "Burninate" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DbMetaAnswer_MetaQuestionId",
                table: "DbMetaAnswer",
                column: "MetaQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_DbMetaAnswerStatistics_MetaAnswerId",
                table: "DbMetaAnswerStatistics",
                column: "MetaAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_DbMetaQuestionStatistics_MetaQuestionId",
                table: "DbMetaQuestionStatistics",
                column: "MetaQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_DbMetaQuestionTag_RequestTypeId",
                table: "DbMetaQuestionTag",
                column: "RequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DbMetaQuestionTag_SecondaryTagName",
                table: "DbMetaQuestionTag",
                column: "SecondaryTagName");

            migrationBuilder.CreateIndex(
                name: "IX_DbMetaQuestionTag_TagName",
                table: "DbMetaQuestionTag",
                column: "TagName");

            migrationBuilder.CreateIndex(
                name: "IX_DbTagStatistics_TagName",
                table: "DbTagStatistics",
                column: "TagName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DbMetaAnswerStatistics");

            migrationBuilder.DropTable(
                name: "DbMetaQuestionStatistics");

            migrationBuilder.DropTable(
                name: "DbMetaQuestionTag");

            migrationBuilder.DropTable(
                name: "DbTagStatistics");

            migrationBuilder.DropTable(
                name: "DbMetaAnswer");

            migrationBuilder.DropTable(
                name: "DbRequestType");

            migrationBuilder.DropTable(
                name: "DbTag");

            migrationBuilder.DropTable(
                name: "DbMetaQuestion");
        }
    }
}
