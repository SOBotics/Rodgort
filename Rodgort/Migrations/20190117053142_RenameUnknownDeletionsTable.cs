using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class RenameUnknownDeletionsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DbUnknownDeletions_SiteUsers_ProcessedByUserId",
                table: "DbUnknownDeletions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActions_DbUnknownDeletions_UnknownDeletionId",
                table: "UserActions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DbUnknownDeletions",
                table: "DbUnknownDeletions");

            migrationBuilder.RenameTable(
                name: "DbUnknownDeletions",
                newName: "UnknownDeletions");

            migrationBuilder.RenameIndex(
                name: "IX_DbUnknownDeletions_ProcessedByUserId",
                table: "UnknownDeletions",
                newName: "IX_UnknownDeletions_ProcessedByUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UnknownDeletions",
                table: "UnknownDeletions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UnknownDeletions_SiteUsers_ProcessedByUserId",
                table: "UnknownDeletions",
                column: "ProcessedByUserId",
                principalTable: "SiteUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActions_UnknownDeletions_UnknownDeletionId",
                table: "UserActions",
                column: "UnknownDeletionId",
                principalTable: "UnknownDeletions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnknownDeletions_SiteUsers_ProcessedByUserId",
                table: "UnknownDeletions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActions_UnknownDeletions_UnknownDeletionId",
                table: "UserActions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UnknownDeletions",
                table: "UnknownDeletions");

            migrationBuilder.RenameTable(
                name: "UnknownDeletions",
                newName: "DbUnknownDeletions");

            migrationBuilder.RenameIndex(
                name: "IX_UnknownDeletions_ProcessedByUserId",
                table: "DbUnknownDeletions",
                newName: "IX_DbUnknownDeletions_ProcessedByUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DbUnknownDeletions",
                table: "DbUnknownDeletions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DbUnknownDeletions_SiteUsers_ProcessedByUserId",
                table: "DbUnknownDeletions",
                column: "ProcessedByUserId",
                principalTable: "SiteUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActions_DbUnknownDeletions_UnknownDeletionId",
                table: "UserActions",
                column: "UnknownDeletionId",
                principalTable: "DbUnknownDeletions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
