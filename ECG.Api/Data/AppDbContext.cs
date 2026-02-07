using ECG.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ECG.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<EcgCase> Cases => Set<EcgCase>();
        public DbSet<EcgCaseImage> CaseImages => Set<EcgCaseImage>();

        public DbSet<EcgCasePrediction> CasePredictions => Set<EcgCasePrediction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

                entity.HasCheckConstraint(
                    "ck_users_role",
                    "\"Role\" IN ('Admin','Technician')"
                );

                entity.Property(x => x.StaffCode).HasMaxLength(50);
                entity.Property(x => x.FullName).HasMaxLength(200);
                entity.Property(x => x.Title).HasMaxLength(100);
                entity.Property(x => x.Department).HasMaxLength(100);

                entity.HasIndex(x => x.StaffCode).IsUnique();
            });

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

                entity.Property(x => x.Gender);

                entity.Property(x => x.IsExamined)
                    .HasDefaultValue(false);

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);

                entity.HasIndex(x => x.IsActive);

                entity.HasCheckConstraint(
                    "ck_patients_name_len",
                    "length(\"Name\") > 2"
                );
            });

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

                entity.Property(x => x.IsDeleted)
                    .HasDefaultValue(false);

                entity.HasIndex(x => x.IsDeleted);

                entity.HasOne(x => x.Patient)
                    .WithMany()
                    .HasForeignKey(x => x.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(x => x.PredictedLabel)
                    .HasMaxLength(20);

                entity.HasOne(x => x.PredictedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.PredictedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.PatientId);
                entity.HasIndex(x => x.MeasuredAt);
                entity.HasIndex(x => x.Status);
                entity.HasIndex(x => x.CreatedByUserId);
                entity.HasIndex(x => x.PredictedAt);
                entity.HasIndex(x => x.PredictedByUserId);

                entity.HasQueryFilter(x => !x.IsDeleted);
            });

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

            modelBuilder.Entity<EcgCasePrediction>(entity =>
            {
                entity.ToTable("ecg_case_predictions");

                entity.Property(x => x.Label)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasCheckConstraint(
                    "ck_ecg_case_predictions_label",
                    "\"Label\" IN ('MI','non-MI','uncertain')"
                );

                entity.Property(x => x.Algorithm)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.Note)
                    .HasMaxLength(500);

                entity.HasOne(x => x.Case)
                    .WithMany(c => c.Predictions)
                    .HasForeignKey(x => x.CaseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.PredictedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.PredictedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.CaseId);
                entity.HasIndex(x => x.PredictedAt);
                entity.HasIndex(x => x.PredictedByUserId);
            });
        }
    }
}
