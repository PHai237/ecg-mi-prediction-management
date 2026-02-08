using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECG.Api.Migrations
{
    public partial class UpdatePatientsGenderToBoolAndExamined : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Drop constraint cũ (gender text enum) nếu có
            migrationBuilder.Sql(@"ALTER TABLE patients DROP CONSTRAINT IF EXISTS ck_patients_gender;");

            // 2) Cho phép Gender nullable trước (vì 'khac' sẽ thành null)
            migrationBuilder.Sql(@"ALTER TABLE patients ALTER COLUMN ""Gender"" DROP NOT NULL;");

            // 3) Đổi kiểu Gender: text -> boolean với USING CASE
            migrationBuilder.Sql(@"
                ALTER TABLE patients
                ALTER COLUMN ""Gender"" TYPE boolean
                USING (
                    CASE
                        WHEN ""Gender"" = 'nam' THEN TRUE
                        WHEN ""Gender"" = 'nu' THEN FALSE
                        ELSE NULL
                    END
                );
            ");

            // 4) Thêm cột IsExamined (default false)
            migrationBuilder.AddColumn<bool>(
                name: "IsExamined",
                table: "patients",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop IsExamined
            migrationBuilder.DropColumn(
                name: "IsExamined",
                table: "patients"
            );

            // boolean -> text
            migrationBuilder.Sql(@"
                ALTER TABLE patients
                ALTER COLUMN ""Gender"" TYPE text
                USING (
                    CASE
                        WHEN ""Gender"" IS TRUE THEN 'nam'
                        WHEN ""Gender"" IS FALSE THEN 'nu'
                        ELSE 'khac'
                    END
                );
            ");

            // set NOT NULL lại như schema cũ
            migrationBuilder.Sql(@"ALTER TABLE patients ALTER COLUMN ""Gender"" SET NOT NULL;");

            // add lại constraint cũ
            migrationBuilder.Sql(@"
                ALTER TABLE patients
                ADD CONSTRAINT ck_patients_gender CHECK (""Gender"" IN ('nam','nu','khac'));
            ");
        }
    }
}
