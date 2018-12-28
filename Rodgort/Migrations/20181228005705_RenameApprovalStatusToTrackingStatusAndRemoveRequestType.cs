using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class RenameApprovalStatusToTrackingStatusAndRemoveRequestType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionTags_RequestTypes_RequestTypeId",
                table: "MetaQuestionTags");

            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionTags_MetaQuestionTagStatuses_StatusId",
                table: "MetaQuestionTags");

            migrationBuilder.DropTable(
                name: "RequestTypes");

            migrationBuilder.DropIndex(
                name: "IX_MetaQuestionTags_RequestTypeId",
                table: "MetaQuestionTags");

            migrationBuilder.DropColumn(
                name: "RequestTypeId",
                table: "MetaQuestionTags");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "MetaQuestionTags",
                newName: "TrackingStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_MetaQuestionTags_StatusId",
                table: "MetaQuestionTags",
                newName: "IX_MetaQuestionTags_TrackingStatusId");

            migrationBuilder.UpdateData(
                table: "MetaQuestionTagStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "None");

            migrationBuilder.UpdateData(
                table: "MetaQuestionTagStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Tracked");

            migrationBuilder.UpdateData(
                table: "MetaQuestionTagStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Ignored");

            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionTags_MetaQuestionTagStatuses_TrackingStatusId",
                table: "MetaQuestionTags",
                column: "TrackingStatusId",
                principalTable: "MetaQuestionTagStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetaQuestionTags_MetaQuestionTagStatuses_TrackingStatusId",
                table: "MetaQuestionTags");

            migrationBuilder.RenameColumn(
                name: "TrackingStatusId",
                table: "MetaQuestionTags",
                newName: "StatusId");

            migrationBuilder.RenameIndex(
                name: "IX_MetaQuestionTags_TrackingStatusId",
                table: "MetaQuestionTags",
                newName: "IX_MetaQuestionTags_StatusId");

            migrationBuilder.AddColumn<int>(
                name: "RequestTypeId",
                table: "MetaQuestionTags",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.UpdateData(
                table: "MetaQuestionTagStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Pending");

            migrationBuilder.UpdateData(
                table: "MetaQuestionTagStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Approved");

            migrationBuilder.UpdateData(
                table: "MetaQuestionTagStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Rejected");

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
                name: "IX_MetaQuestionTags_RequestTypeId",
                table: "MetaQuestionTags",
                column: "RequestTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionTags_RequestTypes_RequestTypeId",
                table: "MetaQuestionTags",
                column: "RequestTypeId",
                principalTable: "RequestTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MetaQuestionTags_MetaQuestionTagStatuses_StatusId",
                table: "MetaQuestionTags",
                column: "StatusId",
                principalTable: "MetaQuestionTagStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
