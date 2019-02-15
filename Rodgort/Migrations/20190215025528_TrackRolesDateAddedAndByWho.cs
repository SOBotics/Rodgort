using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Rodgort.Migrations
{
    public partial class TrackRolesDateAddedAndByWho : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AddedByUserId",
                table: "SiteUserRoles",
                nullable: false,
                defaultValue: -1);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAdded",
                table: "SiteUserRoles",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'");

            migrationBuilder.CreateIndex(
                name: "IX_SiteUserRoles_AddedByUserId",
                table: "SiteUserRoles",
                column: "AddedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SiteUserRoles_SiteUsers_AddedByUserId",
                table: "SiteUserRoles",
                column: "AddedByUserId",
                principalTable: "SiteUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SiteUserRoles_SiteUsers_AddedByUserId",
                table: "SiteUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_SiteUserRoles_AddedByUserId",
                table: "SiteUserRoles");

            migrationBuilder.DropColumn(
                name: "AddedByUserId",
                table: "SiteUserRoles");

            migrationBuilder.DropColumn(
                name: "DateAdded",
                table: "SiteUserRoles");
        }
    }
}
