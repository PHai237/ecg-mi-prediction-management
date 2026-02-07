namespace ECG.Api.Models
{
    public class EcgCase
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        public int? CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        public DateTime MeasuredAt { get; set; }

        public string Status { get; set; } = "new";

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? PredictedLabel { get; set; }
        public double? PredictedConfidence { get; set; }
        public DateTime? PredictedAt { get; set; }

        public int? PredictedByUserId { get; set; }
        public User? PredictedByUser { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        public List<EcgCaseImage> Images { get; set; } = new();
        public List<EcgCasePrediction> Predictions { get; set; } = new();
    }
}
