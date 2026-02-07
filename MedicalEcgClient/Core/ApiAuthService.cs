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
                    // Đọc lỗi chi tiết từ server
                    var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                    _logger.Warning($"[AUDIT] Login Failed ({response.StatusCode}). Detail: {error?.Detail ?? error?.Title}");
                    return null;
                }

                var authResult = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (authResult == null || string.IsNullOrEmpty(authResult.Token)) return null;

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);

                // Call /Me
                var meResponse = await _httpClient.GetAsync("api/auth/me");
                if (meResponse.IsSuccessStatusCode)
                {
                    var userDto = await meResponse.Content.ReadFromJsonAsync<UserDto>();
                    CurrentUser = new User
                    {
                        Username = userDto?.Username ?? username,
                        FullName = userDto?.FullName ?? "Unknown Doctor",
                        Role = userDto?.Role ?? "Doctor",
                        Token = authResult.Token
                    };
                    _logger.Information($"[AUDIT] Login SUCCESS. Welcome {CurrentUser.FullName}");
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
            _logger.Information($"[AUDIT] User {CurrentUser?.Username} logging out.");
            CurrentUser = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}