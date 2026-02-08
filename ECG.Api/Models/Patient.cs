namespace ECG.Api.Models
{
    public class Patient
    {
        public int Id { get; set; }

        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;

        public DateOnly DateOfBirth { get; set; }

        public bool? Gender { get; set; }

        public bool IsExamined { get; set; } = false;

        public string? Note { get; set; }

        // Soft deactivate (hospital-style)
        public bool IsActive { get; set; } = true;
        public DateTime? DeactivatedAt { get; set; }
        public int? DeactivatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
