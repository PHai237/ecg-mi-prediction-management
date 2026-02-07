using ECG.Api.Models;
using ECG.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace ECG.Api.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            await db.Database.MigrateAsync();

            if (!await db.Users.AnyAsync())
            {
                db.Users.AddRange(
                    new User
                    {
                        Username = "admin",
                        PasswordHash = PasswordHasher.Hash("Admin@123"),
                        Role = "Admin",
                        StaffCode = "BS001",
                        FullName = "Admin Doctor",
                        Title = "Doctor",
                        Department = "Cardiology"
                    },
                    new User
                    {
                        Username = "tech",
                        PasswordHash = PasswordHasher.Hash("Tech@123"),
                        Role = "Technician",
                        StaffCode = "KT001",
                        FullName = "Tech Staff",
                        Title = "Technician",
                        Department = "ECG Lab"
                    }
                );

                await db.SaveChangesAsync();
                return;
            }

            // Backfill nhẹ nếu DB đã có user nhưng thiếu profile
            var admin = await db.Users.FirstOrDefaultAsync(x => x.Username == "admin");
            if (admin != null)
            {
                admin.StaffCode ??= "BS001";
                admin.FullName ??= "Admin Doctor";
                admin.Title ??= "Doctor";
                admin.Department ??= "Cardiology";
            }

            var tech = await db.Users.FirstOrDefaultAsync(x => x.Username == "tech");
            if (tech != null)
            {
                tech.StaffCode ??= "KT001";
                tech.FullName ??= "Tech Staff";
                tech.Title ??= "Technician";
                tech.Department ??= "ECG Lab";
            }

            await db.SaveChangesAsync();
        }
    }
}
