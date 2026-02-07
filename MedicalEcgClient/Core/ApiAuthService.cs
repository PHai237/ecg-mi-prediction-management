using MedicalEcgClient.Core.Dto;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MedicalEcgClient.Core
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string username, string password);
        void Logout();
        User? CurrentUser { get; }
    }

    // ---------------------------------------------------------
    // 1. REAL AUTH SERVICE (Kết nối Login API)
    // ---------------------------------------------------------
    public class ApiAuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        public User? CurrentUser { get; private set; }

        public ApiAuthService(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            try
            {
                _logger.Information($"[AUDIT] Sending Login Request for user: {username}");

                var loginPayload = new LoginRequest { Username = username, Password = password };
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginPayload);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                    _logger.Warning($"[AUDIT] Login Failed ({response.StatusCode}). Detail: {error?.Detail ?? error?.Title}");
                    return null;
                }

                var authResult = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (authResult == null || string.IsNullOrEmpty(authResult.Token))
                {
                    _logger.Error("[CRITICAL] Login success but Token/User data is empty.");
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);

                if (authResult.User != null)
                {
                    CurrentUser = new User
                    {
                        Id = authResult.User.Id.ToString(),
                        Username = authResult.User.Username,
                        FullName = authResult.User.FullName,
                        Role = authResult.User.Role,
                        Token = authResult.Token,
                        StaffCode = authResult.User.StaffCode,
                        Title = authResult.User.Title,
                        Department = authResult.User.Department
                    };

                    _logger.Information($"[AUDIT] Login SUCCESS. Welcome {CurrentUser.FullName} ({CurrentUser.StaffCode})");
                    return CurrentUser;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[CRITICAL] Login API Exception");
                return null;
            }
        }

        public void Logout()
        {
            if (CurrentUser != null)
                _logger.Information($"[AUDIT] User {CurrentUser.Username} logging out.");

            CurrentUser = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}