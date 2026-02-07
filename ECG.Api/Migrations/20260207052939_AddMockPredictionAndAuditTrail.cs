using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ECG.Api.Migrations
{
    public partial class AddMockPredictionAndAuditTrail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add latest prediction summary columns to ecg_cases
            migrationBuilder.AddColumn<string>(
                name: "PredictedLabel",
                table: "ecg_cases",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PredictedConfidence",
                table: "ecg_cases",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PredictedAt",
                table: "ecg_cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PredictedByUserId",
                table: "ecg_cases",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ecg_cases_PredictedAt",
                table: "ecg_cases",
                column: "PredictedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ecg_cases_PredictedByUserId",
                table: "ecg_cases",
                column: "PredictedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ecg_cases_users_PredictedByUserId",
                table: "ecg_cases",
                column: "PredictedByUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Create ecg_case_predictions table
            migrationBuilder.CreateTable(
                name: "ecg_case_predictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CaseId = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    Algorithm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PredictedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PredictedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ecg_case_predictions", x => x.Id);
                    table.CheckConstraint("ck_ecg_case_predictions_label", "\"Label\" IN ('MI','non-MI','uncertain')");
                    table.ForeignKey(
                        name: "FK_ecg_case_predictions_ecg_cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "ecg_cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ecg_case_predictions_users_PredictedByUserId",
                        column: x => x.PredictedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ecg_case_predictions_CaseId",
                table: "ecg_case_predictions",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ecg_case_predictions_PredictedAt",
                table: "ecg_case_predictions",
                column: "PredictedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ecg_case_predictions_PredictedByUserId",
                table: "ecg_case_predictions",
                column: "PredictedByUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ecg_case_predictions");

            migrationBuilder.DropForeignKey(
                name: "FK_ecg_cases_users_PredictedByUserId",
                table: "ecg_cases");

            migrationBuilder.DropIndex(
                name: "IX_ecg_cases_PredictedAt",
                table: "ecg_cases");

            migrationBuilder.DropIndex(
                name: "IX_ecg_cases_PredictedByUserId",
                table: "ecg_cases");

            migrationBuilder.DropColumn(
                name: "PredictedLabel",
                table: "ecg_cases");

            migrationBuilder.DropColumn(
                name: "PredictedConfidence",
                table: "ecg_cases");

            migrationBuilder.DropColumn(
                name: "PredictedAt",
                table: "ecg_cases");

            migrationBuilder.DropColumn(
                name: "PredictedByUserId",
                table: "ecg_cases");
        }
    }
}
