using ECG.Api.Models;
using ECG.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace ECG.Api.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            // Apply any pending migrations
            await db.Database.MigrateAsync();

            // Seed default users if empty
            if (!await db.Users.AnyAsync())
            {
                db.Users.AddRange(
                    new User
                    {
                        Username = "admin",
                        PasswordHash = PasswordHasher.Hash("Admin@123"),
                        Role = "Admin"
                    },
                    new User
                    {
                        Username = "tech",
                        PasswordHash = PasswordHasher.Hash("Tech@123"),
                        Role = "Technician"
                    }
                );

                await db.SaveChangesAsync();
            }
        }
    }
}
