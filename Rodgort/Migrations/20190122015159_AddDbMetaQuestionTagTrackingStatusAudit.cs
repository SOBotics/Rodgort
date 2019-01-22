using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Rodgort.Migrations
{
    public partial class AddDbMetaQuestionTagTrackingStatusAudit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MetaQuestionTagTrackingStatusAudits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    TimeChanged = table.Column<DateTime>(nullable: false),
                    PreviousTrackingStatusId = table.Column<int>(nullable: true),
                    NewTrackingStatusId = table.Column<int>(nullable: false),
                    ChangedByUserId = table.Column<int>(nullable: false),
                    MetaQuestionId = table.Column<int>(nullable: false),
                    Tag = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaQuestionTagTrackingStatusAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetaQuestionTagTrackingStatusAudits_SiteUsers_ChangedByUser~",
                        column: x => x.ChangedByUserId,
                        principalTable: "SiteUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetaQuestionTagTrackingStatusAudits_MetaQuestionTagStatuses~",
                        column: x => x.NewTrackingStatusId,
                        principalTable: "MetaQuestionTagStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetaQuestionTagTrackingStatusAudits_MetaQuestionTagStatuse~1",
                        column: x => x.PreviousTrackingStatusId,
                        principalTable: "MetaQuestionTagStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MetaQuestionTagTrackingStatusAudits_MetaQuestionTags_MetaQu~",
                        columns: x => new { x.MetaQuestionId, x.Tag },
                        principalTable: "MetaQuestionTags",
                        principalColumns: new[] { "MetaQuestionId", "TagName" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetaQuestionTagTrackingStatusAudits_ChangedByUserId",
                table: "MetaQuestionTagTrackingStatusAudits",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaQuestionTagTrackingStatusAudits_NewTrackingStatusId",
                table: "MetaQuestionTagTrackingStatusAudits",
                column: "NewTrackingStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaQuestionTagTrackingStatusAudits_PreviousTrackingStatusId",
                table: "MetaQuestionTagTrackingStatusAudits",
                column: "PreviousTrackingStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaQuestionTagTrackingStatusAudits_MetaQuestionId_Tag",
                table: "MetaQuestionTagTrackingStatusAudits",
                columns: new[] { "MetaQuestionId", "Tag" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetaQuestionTagTrackingStatusAudits");
        }
    }
}
