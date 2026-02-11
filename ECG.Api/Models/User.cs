namespace ECG.Api.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        // "Admin" | "Technician"
        public string Role { get; set; } = "Technician";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
