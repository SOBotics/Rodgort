using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Rodgort.Migrations
{
    public partial class MoveRolesToIdPrimaryKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM site_user_roles;
DELETE FROM site_user_role_audits;
DELETE FROM roles;
");

            migrationBuilder.DropForeignKey(
                name: "fk_site_user_role_audits_site_user_roles_role_temp_id",
                table: "site_user_role_audits");

            migrationBuilder.DropForeignKey(
                name: "fk_site_user_roles_roles_role_temp_id",
                table: "site_user_roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_site_user_roles",
                table: "site_user_roles");

            migrationBuilder.DropIndex(
                name: "IX_site_user_roles_role_name",
                table: "site_user_roles");

            migrationBuilder.DropIndex(
                name: "IX_site_user_role_audits_user_id_role_name",
                table: "site_user_role_audits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_roles",
                table: "roles");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "name",
                keyValue: "Moderator");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "name",
                keyValue: "Rodgort Admin");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "name",
                keyValue: "Trogdor Room Owner");

            migrationBuilder.DropColumn(
                name: "role_name",
                table: "site_user_roles");

            migrationBuilder.DropColumn(
                name: "role_name",
                table: "site_user_role_audits");

            migrationBuilder.AddColumn<int>(
                name: "role_id",
                table: "site_user_roles",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "role_id",
                table: "site_user_role_audits",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "roles",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "roles",
                nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_site_user_roles",
                table: "site_user_roles",
                columns: new[] { "user_id", "role_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_roles",
                table: "roles",
                column: "id");

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "name" },
                values: new object[] { 1, "Super user" });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "name" },
                values: new object[] { 2, "Admin" });

            migrationBuilder.CreateIndex(
                name: "ix_site_user_roles_role_id",
                table: "site_user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_site_user_role_audits_user_id_role_id",
                table: "site_user_role_audits",
                columns: new[] { "user_id", "role_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_site_user_role_audits_site_user_roles_role_id",
                table: "site_user_role_audits",
                columns: new[] { "user_id", "role_id" },
                principalTable: "site_user_roles",
                principalColumns: new[] { "user_id", "role_id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_site_user_roles_roles_role_id",
                table: "site_user_roles",
                column: "role_id",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_site_user_role_audits_site_user_roles_role_id",
                table: "site_user_role_audits");

            migrationBuilder.DropForeignKey(
                name: "fk_site_user_roles_roles_role_id",
                table: "site_user_roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_site_user_roles",
                table: "site_user_roles");

            migrationBuilder.DropIndex(
                name: "ix_site_user_roles_role_id",
                table: "site_user_roles");

            migrationBuilder.DropIndex(
                name: "IX_site_user_role_audits_user_id_role_id",
                table: "site_user_role_audits");

            migrationBuilder.DropPrimaryKey(
                name: "pk_roles",
                table: "roles");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "role_id",
                table: "site_user_roles");

            migrationBuilder.DropColumn(
                name: "role_id",
                table: "site_user_role_audits");

            migrationBuilder.DropColumn(
                name: "id",
                table: "roles");

            migrationBuilder.AddColumn<string>(
                name: "role_name",
                table: "site_user_roles",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "role_name",
                table: "site_user_role_audits",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "roles",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_site_user_roles",
                table: "site_user_roles",
                columns: new[] { "user_id", "role_name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_roles",
                table: "roles",
                column: "name");

            migrationBuilder.InsertData(
                table: "roles",
                column: "name",
                value: "Trogdor Room Owner");

            migrationBuilder.InsertData(
                table: "roles",
                column: "name",
                value: "Moderator");

            migrationBuilder.InsertData(
                table: "roles",
                column: "name",
                value: "Rodgort Admin");

            migrationBuilder.CreateIndex(
                name: "IX_site_user_roles_role_name",
                table: "site_user_roles",
                column: "role_name");

            migrationBuilder.CreateIndex(
                name: "IX_site_user_role_audits_user_id_role_name",
                table: "site_user_role_audits",
                columns: new[] { "user_id", "role_name" });

            migrationBuilder.AddForeignKey(
                name: "fk_site_user_role_audits_site_user_roles_role_temp_id",
                table: "site_user_role_audits",
                columns: new[] { "user_id", "role_name" },
                principalTable: "site_user_roles",
                principalColumns: new[] { "user_id", "role_name" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_site_user_roles_roles_role_temp_id",
                table: "site_user_roles",
                column: "role_name",
                principalTable: "roles",
                principalColumn: "name",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
