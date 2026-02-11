namespace ECG.Api.Models
{
    public class Patient
    {
        public int Id { get; set; }

        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;

        public DateOnly DateOfBirth { get; set; }

        public string Gender { get; set; } = null!;

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
