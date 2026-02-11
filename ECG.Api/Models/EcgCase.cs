using System.ComponentModel.DataAnnotations;

namespace ECG.Api.Models
{
    public class EcgCase
    {
        public int Id { get; set; }

        // FK -> Patient
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        // Thời điểm đo (UTC)
        public DateTime MeasuredAt { get; set; }

        // new / uploaded / predicted
        public string Status { get; set; } = "new";

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public List<EcgCaseImage> Images { get; set; } = new();
    }
}
