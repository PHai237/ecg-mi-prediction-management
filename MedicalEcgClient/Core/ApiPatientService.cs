using MedicalEcgClient.Core.Dto;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace MedicalEcgClient.Core
{
    public interface IPatientService
    {
        Task<List<Patient>> GetDailyPatientsAsync(string doctorId);
        Task<bool> CreatePatientAsync(Patient patient);
        Task<bool> UpdatePatientAsync(Patient patient);
        Task<bool> DeletePatientAsync(string patientId);
    }

    public class ApiPatientService : IPatientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IAuthService _authService;

        public ApiPatientService(HttpClient httpClient, ILogger logger, IAuthService authService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _authService = authService;
        }

        private void EnsureAuthHeader()
        {
            var token = _authService.CurrentUser?.Token;
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<Patient>> GetDailyPatientsAsync(string doctorId)
        {
            try
            {
                EnsureAuthHeader();
                var response = await _httpClient.GetAsync("api/patients");

                if (response.IsSuccessStatusCode)
                {
                    var dtos = await response.Content.ReadFromJsonAsync<List<PatientDto>>();
                    if (dtos == null) return new List<Patient>();

                    return dtos.Select(d => new Patient
                    {
                        Id = d.Id.ToString(),
                        PatientCode = d.Code,
                        FullName = d.Name,
                        DateOfBirth = d.DateOfBirth ?? new DateTime(2000, 1, 1),
                        Gender = d.Gender switch { true => "Nam", false => "Nữ", _ => "Khác" },
                        Note = d.Note ?? "",
                        Status = d.IsExamined ? "Đã khám" : "Chờ khám"
                    }).OrderBy(p => p.Status).ToList();
                }
                return new List<Patient>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[API] GetPatients Exception");
                return new List<Patient>();
            }
        }

        public async Task<bool> CreatePatientAsync(Patient patient)
        {
            try
            {
                EnsureAuthHeader();
                bool? genderBool = patient.Gender switch { "Nam" => true, "Nữ" => false, _ => null };
                string dobString = patient.DateOfBirth.ToString("yyyy-MM-dd");

                bool isExamined = patient.Status == "Đã khám";

                var requestBody = new CreatePatientRequest
                {
                    Code = patient.PatientCode,
                    Name = patient.FullName,
                    DateOfBirth = dobString,
                    Gender = genderBool,
                    IsExamined = false,
                    Note = patient.Note
                };

                var response = await _httpClient.PostAsJsonAsync("api/patients", requestBody);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> UpdatePatientAsync(Patient patient)
        {
            try
            {
                EnsureAuthHeader();
                bool? genderBool = patient.Gender switch { "Nam" => true, "Nữ" => false, _ => null };
                string dobString = patient.DateOfBirth.ToString("yyyy-MM-dd");

                var requestBody = new UpdatePatientRequest
                {
                    Name = patient.FullName,
                    DateOfBirth = dobString,
                    Gender = genderBool,
                    IsExamined = patient.Status == "Đã khám",
                    Note = patient.Note
                };

                var response = await _httpClient.PutAsJsonAsync($"api/patients/{patient.Id}", requestBody);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> DeletePatientAsync(string patientId)
        {
            try
            {
                EnsureAuthHeader();
                var response = await _httpClient.DeleteAsync($"api/patients/{patientId}");
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}
