namespace ECG.Api.Models
{
    public class EcgCaseImage
    {
        public int Id { get; set; }

        // FK -> EcgCase
        public int CaseId { get; set; }
        public EcgCase? Case { get; set; }

        // Lưu file
        public string FileName { get; set; } = string.Empty;          // tên file đã lưu (guid + ext)
        public string OriginalFileName { get; set; } = string.Empty;  // tên file gốc
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }

        // URL public để FE/WPF load
        public string UrlPath { get; set; } = string.Empty; // ví dụ: /uploads/cases/12/abc.jpg

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
