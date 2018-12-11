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
                name: "MetaQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Link = table.Column<string>(nullable: true),
                    LastSeen = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaQuestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetaTags",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaTags", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "RequestTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "MetaAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    MetaQuestionId = table.Column<int>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    LastSeen = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetaAnswers_MetaQuestions_MetaQuestionId",
                        column: x => x.MetaQuestionId,
                        principalTable: "MetaQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetaQuestionStatistics",
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
                    table.PrimaryKey("PK_MetaQuestionStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetaQuestionStatistics_MetaQuestions_MetaQuestionId",
                        column: x => x.MetaQuestionId,
                        principalTable: "MetaQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetaQuestionMetaTags",
                columns: table => new
                {
                    MetaQuestionId = table.Column<int>(nullable: false),
                    TagName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaQuestionMetaTags", x => new { x.MetaQuestionId, x.TagName });
                    table.ForeignKey(
                        name: "FK_MetaQuestionMetaTags_MetaQuestions_MetaQuestionId",
                        column: x => x.MetaQuestionId,
                        principalTable: "MetaQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetaQuestionMetaTags_MetaTags_TagName",
                        column: x => x.TagName,
                        principalTable: "MetaTags",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetaQuestionTags",
                columns: table => new
                {
                    MetaQuestionId = table.Column<int>(nullable: false),
                    TagName = table.Column<string>(nullable: false),
                    RequestTypeId = table.Column<int>(nullable: false),
                    SecondaryTagName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaQuestionTags", x => new { x.MetaQuestionId, x.TagName });
                    table.ForeignKey(
                        name: "FK_MetaQuestionTags_MetaQuestions_MetaQuestionId",
                        column: x => x.MetaQuestionId,
                        principalTable: "MetaQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetaQuestionTags_RequestTypes_RequestTypeId",
                        column: x => x.RequestTypeId,
                        principalTable: "RequestTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetaQuestionTags_Tags_SecondaryTagName",
                        column: x => x.SecondaryTagName,
                        principalTable: "Tags",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MetaQuestionTags_Tags_TagName",
                        column: x => x.TagName,
                        principalTable: "Tags",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagStatistics",
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
                    table.PrimaryKey("PK_TagStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagStatistics_Tags_TagName",
                        column: x => x.TagName,
                        principalTable: "Tags",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MetaAnswerStatistics",
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
                    table.PrimaryKey("PK_MetaAnswerStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetaAnswerStatistics_MetaAnswers_MetaAnswerId",
                        column: x => x.MetaAnswerId,
                        principalTable: "MetaAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MetaTags",
                column: "Name",
                values: new object[]
                {
                    "status-completed",
                    "status-planned",
                    "status-declined"
                });

            migrationBuilder.InsertData(
                table: "RequestTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Unknown" },
                    { 2, "Synonym" },
                    { 3, "Merge" },
                    { 4, "Burninate" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetaAnswers_MetaQuestionId",
                table: "MetaAnswers",
                column: "MetaQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaAnswerStatistics_MetaAnswerId",
                table: "MetaAnswerStatistics",
                column: "MetaAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaQuestionMetaTags_TagName",
                table: "MetaQuestionMetaTags",
                column: "TagName");

            migrationBuilder.CreateIndex(
                name: "IX_MetaQuestionStatistics_MetaQuestionId",
                table: "MetaQuestionStatistics",
                column: "MetaQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaQuestionTags_RequestTypeId",
                table: "MetaQuestionTags",
                column: "RequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaQuestionTags_SecondaryTagName",
                table: "MetaQuestionTags",
                column: "SecondaryTagName");

            migrationBuilder.CreateIndex(
                name: "IX_MetaQuestionTags_TagName",
                table: "MetaQuestionTags",
                column: "TagName");

            migrationBuilder.CreateIndex(
                name: "IX_TagStatistics_TagName",
                table: "TagStatistics",
                column: "TagName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetaAnswerStatistics");

            migrationBuilder.DropTable(
                name: "MetaQuestionMetaTags");

            migrationBuilder.DropTable(
                name: "MetaQuestionStatistics");

            migrationBuilder.DropTable(
                name: "MetaQuestionTags");

            migrationBuilder.DropTable(
                name: "TagStatistics");

            migrationBuilder.DropTable(
                name: "MetaAnswers");

            migrationBuilder.DropTable(
                name: "MetaTags");

            migrationBuilder.DropTable(
                name: "RequestTypes");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "MetaQuestions");
        }
    }
}
