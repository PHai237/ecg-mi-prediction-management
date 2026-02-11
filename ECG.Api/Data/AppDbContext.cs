using ECG.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ECG.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Auth
        public DbSet<User> Users => Set<User>();

        // Patients
        public DbSet<Patient> Patients => Set<Patient>();

        // Milestone B
        public DbSet<EcgCase> Cases => Set<EcgCase>();
        public DbSet<EcgCaseImage> CaseImages => Set<EcgCaseImage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // users (nếu bạn đã cấu hình User ở chỗ khác thì vẫn để, EF mặc định cũng chạy được)
            // =========================
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.Property(x => x.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(x => x.Username)
                    .IsUnique();

                entity.Property(x => x.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Role)
                    .IsRequired()
                    .HasMaxLength(30);
            });

            // =========================
            // patients
            // =========================
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("patients");

                entity.Property(x => x.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(x => x.Code)
                    .IsUnique();

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Gender)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.HasCheckConstraint(
                    "ck_patients_gender",
                    "\"Gender\" IN ('nam','nu','khac')"
                );

                entity.HasCheckConstraint(
                    "ck_patients_name_len",
                    "length(\"Name\") > 2"
                );
            });

            // =========================
            // ecg_cases
            // =========================
            modelBuilder.Entity<EcgCase>(entity =>
            {
                entity.ToTable("ecg_cases");

                entity.Property(x => x.Status)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasCheckConstraint(
                    "ck_ecg_cases_status",
                    "\"Status\" IN ('new','uploaded','predicted')"
                );

                entity.Property(x => x.Note)
                    .HasMaxLength(1000);

                entity.HasOne(x => x.Patient)
                    .WithMany()
                    .HasForeignKey(x => x.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.PatientId);
                entity.HasIndex(x => x.MeasuredAt);
                entity.HasIndex(x => x.Status);
            });

            // =========================
            // ecg_case_images
            // =========================
            modelBuilder.Entity<EcgCaseImage>(entity =>
            {
                entity.ToTable("ecg_case_images");

                entity.Property(x => x.FileName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.OriginalFileName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(x => x.ContentType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.UrlPath)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(x => x.Case)
                    .WithMany(c => c.Images)
                    .HasForeignKey(x => x.CaseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.CaseId);
            });
        }
    }
}
