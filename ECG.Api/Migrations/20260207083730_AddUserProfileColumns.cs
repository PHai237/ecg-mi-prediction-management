using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECG.Api.Migrations
{
    public partial class AddUserProfileColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StaffCode",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_StaffCode",
                table: "users",
                column: "StaffCode",
                unique: true,
                filter: "\"StaffCode\" IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_StaffCode",
                table: "users");

            migrationBuilder.DropColumn(
                name: "StaffCode",
                table: "users");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "users");
        }
    }
}
