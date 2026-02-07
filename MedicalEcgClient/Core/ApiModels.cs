using System;
using System.Text.Json.Serialization;

namespace MedicalEcgClient.Core.Dto
{
    public class LoginRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        [JsonPropertyName("user")]
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("staffCode")]
        public string StaffCode { get; set; } = string.Empty;

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("department")]
        public string Department { get; set; } = string.Empty;
    }

    public class PatientDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("dateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [JsonPropertyName("gender")]
        public bool? Gender { get; set; }

        [JsonPropertyName("isExamined")]
        public bool IsExamined { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePatientRequest
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("dateOfBirth")]
        public string DateOfBirth { get; set; } = string.Empty;

        [JsonPropertyName("gender")]
        public bool? Gender { get; set; }

        [JsonPropertyName("isExamined")]
        public bool IsExamined { get; set; }

        [JsonPropertyName("note")]
        public string Note { get; set; } = string.Empty;
    }

    public class UpdatePatientRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("dateOfBirth")]
        public string DateOfBirth { get; set; } = string.Empty;

        [JsonPropertyName("gender")]
        public bool? Gender { get; set; }

        [JsonPropertyName("isExamined")]
        public bool IsExamined { get; set; }

        [JsonPropertyName("note")]
        public string Note { get; set; } = string.Empty;
    }

    public class CaseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("patientId")]
        public int PatientId { get; set; }

        [JsonPropertyName("patientCode")]
        public string PatientCode { get; set; } = string.Empty;

        [JsonPropertyName("patientName")]
        public string PatientName { get; set; } = string.Empty;

        [JsonPropertyName("measuredAt")]
        public DateTime MeasuredAt { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("note")]
        public string Note { get; set; } = string.Empty;

        [JsonPropertyName("imageCount")]
        public int ImageCount { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("images")]
        public List<CaseImageDto>? Images { get; set; }
    }

    public class CreateCaseRequest
    {
        [JsonPropertyName("patientId")]
        public int PatientId { get; set; }

        [JsonPropertyName("measuredAt")]
        public DateTime MeasuredAt { get; set; }

        [JsonPropertyName("note")]
        public string Note { get; set; } = string.Empty;
    }

    public class CaseImageDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("originalFileName")]
        public string OriginalFileName { get; set; } = string.Empty;

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; } = string.Empty;

        [JsonPropertyName("sizeBytes")]
        public long SizeBytes { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("uploadedAt")]
        public DateTime UploadedAt { get; set; }
    }

    public class ApiErrorResponse
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("detail")]
        public string Detail { get; set; } = string.Empty;

        [JsonPropertyName("instance")]
        public string Instance { get; set; } = string.Empty;
    }
}