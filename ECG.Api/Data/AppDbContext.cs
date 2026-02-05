using ECG.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ECG.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Tables
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------
            // Patients
            // -------------------------
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

            // -------------------------
            // Users
            // -------------------------
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.Property(x => x.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(x => x.Username)
                    .IsUnique();

                entity.Property(x => x.PasswordHash)
                    .IsRequired();

                entity.Property(x => x.Role)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasCheckConstraint(
                    "ck_users_role",
                    "\"Role\" IN ('Admin','Technician')"
                );
            });
        }
    }
}
