using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Rodgort.Migrations
{
    public partial class AddSecurityAuditLogsAndEnabledFlagOnRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "SiteUserRoles",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SiteUserRoleAudits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    UserId = table.Column<int>(nullable: false),
                    RoleName = table.Column<string>(nullable: true),
                    ChangedByUserId = table.Column<int>(nullable: false),
                    DateChanged = table.Column<DateTime>(nullable: false),
                    Added = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteUserRoleAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteUserRoleAudits_SiteUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "SiteUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SiteUserRoleAudits_SiteUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "SiteUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SiteUserRoleAudits_SiteUserRoles_UserId_RoleName",
                        columns: x => new { x.UserId, x.RoleName },
                        principalTable: "SiteUserRoles",
                        principalColumns: new[] { "UserId", "RoleName" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteUserRoleAudits_ChangedByUserId",
                table: "SiteUserRoleAudits",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteUserRoleAudits_UserId_RoleName",
                table: "SiteUserRoleAudits",
                columns: new[] { "UserId", "RoleName" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteUserRoleAudits");

            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "SiteUserRoles");
        }
    }
}
