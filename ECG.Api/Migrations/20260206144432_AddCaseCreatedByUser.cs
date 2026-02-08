using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECG.Api.Migrations
{
    public partial class AddCaseCreatedByUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "ecg_cases",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ecg_cases_CreatedByUserId",
                table: "ecg_cases",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ecg_cases_users_CreatedByUserId",
                table: "ecg_cases",
                column: "CreatedByUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Backfill cho data cũ (nếu có) -> gán admin đầu tiên (nếu tồn tại)
            migrationBuilder.Sql(@"
                UPDATE ecg_cases
                SET ""CreatedByUserId"" = (
                    SELECT ""Id"" FROM users
                    WHERE ""Role"" = 'Admin'
                    ORDER BY ""Id""
                    LIMIT 1
                )
                WHERE ""CreatedByUserId"" IS NULL;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ecg_cases_users_CreatedByUserId",
                table: "ecg_cases");

            migrationBuilder.DropIndex(
                name: "IX_ecg_cases_CreatedByUserId",
                table: "ecg_cases");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ecg_cases");
        }
    }
}
