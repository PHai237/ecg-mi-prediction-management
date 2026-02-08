namespace ECG.Api.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        // "Admin" | "Technician"
        public string Role { get; set; } = "Technician";

        // ===== Profile (hospital staff) =====
        public string? StaffCode { get; set; }      // mã NV (VD: BS001, YT002, KT001...)
        public string? FullName { get; set; }       // họ tên hiển thị
        public string? Title { get; set; }          // chức danh/chức vụ (bác sĩ/y tá/kỹ thuật viên...)
        public string? Department { get; set; }     // khoa/phòng

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
