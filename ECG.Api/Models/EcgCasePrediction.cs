namespace ECG.Api.Models
{
    public class EcgCasePrediction
    {
        public int Id { get; set; }

        public int CaseId { get; set; }
        public EcgCase? Case { get; set; }

        public string Label { get; set; } = "uncertain";
        public double Confidence { get; set; }

        public string Algorithm { get; set; } = "mock-v1";

        public DateTime PredictedAt { get; set; } = DateTime.UtcNow;

        public int PredictedByUserId { get; set; }
        public User? PredictedByUser { get; set; }

        public string? Note { get; set; }
    }
}
