using MedicalEcgClient.Core;
using MedicalEcgClient.Core.Dto;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MedicalEcgClient.Services
{
    public interface ICaseService
    {
        Task<List<CaseDto>> GetHistoryAsync(string patientId);
        Task<CaseDto?> CreateCaseAsync(string patientId, DateTime measuredAt, string note);
        Task<bool> UploadCaseImageAsync(int caseId, BitmapSource image, string fileName);
        Task<bool> UploadCaseImageAsync(int caseId, byte[] imageBytes, string fileName);
        Task<bool> DeleteCaseAsync(int caseId);
        Task<CaseDto?> GetCaseDetailAsync(int caseId);
    }

    public class ApiCaseService : ICaseService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IAuthService _authService;

        public ApiCaseService(HttpClient httpClient, ILogger logger, IAuthService authService)
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

        private async Task LogApiError(HttpResponseMessage response, string context)
        {
            try
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                _logger.Warning($"[API ERROR] {context} | Status: {response.StatusCode} | {error?.Title}: {error?.Detail}");
            }
            catch
            {
                var raw = await response.Content.ReadAsStringAsync();
                _logger.Warning($"[API ERROR] {context} | Status: {response.StatusCode} | Raw: {raw}");
            }
        }

        public async Task<List<CaseDto>> GetHistoryAsync(string patientId)
        {
            try
            {
                EnsureAuthHeader();
                var response = await _httpClient.GetAsync($"api/cases?patientId={patientId}");

                if (response.IsSuccessStatusCode)
                {
                    var cases = await response.Content.ReadFromJsonAsync<List<CaseDto>>();
                    if (cases != null)
                    {
                        foreach (var c in cases)
                        {
                            c.MeasuredAt = c.MeasuredAt.ToLocalTime();
                            c.CreatedAt = c.CreatedAt.ToLocalTime();
                            if (c.Images != null)
                            {
                                foreach (var img in c.Images) img.UploadedAt = img.UploadedAt.ToLocalTime();
                            }
                        }
                        return cases;
                    }
                    return new List<CaseDto>();
                }

                await LogApiError(response, "GetHistory");
                return new List<CaseDto>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetHistory Exception");
                return new List<CaseDto>();
            }
        }

        public async Task<CaseDto?> GetCaseDetailAsync(int caseId)
        {
            try
            {
                EnsureAuthHeader();
                var response = await _httpClient.GetAsync($"api/cases/{caseId}");

                if (response.IsSuccessStatusCode)
                {
                    var c = await response.Content.ReadFromJsonAsync<CaseDto>();
                    if (c != null)
                    {
                        c.MeasuredAt = c.MeasuredAt.ToLocalTime();
                        c.CreatedAt = c.CreatedAt.ToLocalTime();
                        if (c.Images != null)
                        {
                            foreach (var img in c.Images) img.UploadedAt = img.UploadedAt.ToLocalTime();
                        }
                    }
                    return c;
                }

                await LogApiError(response, $"GetDetail({caseId})");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetCaseDetail Exception");
                return null;
            }
        }

        public async Task<CaseDto?> CreateCaseAsync(string patientId, DateTime measuredAt, string note)
        {
            try
            {
                EnsureAuthHeader();

                if (!int.TryParse(patientId, out int pid)) return null;

                var request = new CreateCaseRequest
                {
                    PatientId = pid,
                    MeasuredAt = measuredAt.ToUniversalTime(),
                    Note = note
                };

                var response = await _httpClient.PostAsJsonAsync("api/cases", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CaseDto>();

                    if (result != null)
                    {
                        result.MeasuredAt = result.MeasuredAt.ToLocalTime();
                        result.CreatedAt = result.CreatedAt.ToLocalTime();
                    }
                    return result;
                }

                await LogApiError(response, "CreateCase");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "CreateCase Exception");
                return null;
            }
        }

        public async Task<bool> UploadCaseImageAsync(int caseId, BitmapSource image, string fileName)
        {
            byte[] imageBytes;
            using (var stream = new MemoryStream())
            {
                BitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
                imageBytes = stream.ToArray();
            }
            return await UploadCaseImageAsync(caseId, imageBytes, fileName);
        }

        public async Task<bool> UploadCaseImageAsync(int caseId, byte[] imageBytes, string fileName)
        {
            try
            {
                EnsureAuthHeader();

                using var content = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(imageBytes);

                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                content.Add(fileContent, "files", fileName);

                var response = await _httpClient.PostAsync($"api/cases/{caseId}/images", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.Information($"[AUDIT] Uploaded image for Case {caseId}");
                    return true;
                }

                await LogApiError(response, "UploadImage");
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "UploadImage Exception");
                return false;
            }
        }

        public async Task<bool> DeleteCaseAsync(int caseId)
        {
            try
            {
                EnsureAuthHeader();
                var response = await _httpClient.DeleteAsync($"api/cases/{caseId}");
                if (response.IsSuccessStatusCode) return true;
                await LogApiError(response, "DeleteCase");
                return false;
            }
            catch { return false; }
        }
    }
}