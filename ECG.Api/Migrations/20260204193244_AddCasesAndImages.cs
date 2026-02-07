using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ECG.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCasesAndImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_users_role",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "ecg_cases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    MeasuredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ecg_cases", x => x.Id);
                    table.CheckConstraint("ck_ecg_cases_status", "\"Status\" IN ('new','uploaded','predicted')");
                    table.ForeignKey(
                        name: "FK_ecg_cases_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ecg_case_images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CaseId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UrlPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ecg_case_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ecg_case_images_ecg_cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "ecg_cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ecg_case_images_CaseId",
                table: "ecg_case_images",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ecg_cases_MeasuredAt",
                table: "ecg_cases",
                column: "MeasuredAt");

            migrationBuilder.CreateIndex(
                name: "IX_ecg_cases_PatientId",
                table: "ecg_cases",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ecg_cases_Status",
                table: "ecg_cases",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ecg_case_images");

            migrationBuilder.DropTable(
                name: "ecg_cases");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddCheckConstraint(
                name: "ck_users_role",
                table: "users",
                sql: "\"Role\" IN ('Admin','Technician')");
        }
    }
}
