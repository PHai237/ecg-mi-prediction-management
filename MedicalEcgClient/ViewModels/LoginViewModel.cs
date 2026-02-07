using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicalEcgClient.Core;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace MedicalEcgClient.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _username = "admin";

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public System.Action<User>? OnLoginSuccess;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        public async Task Login(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Vui lòng nhập tên đăng nhập.";
                return;
            }

            string password = "";
            if (parameter is PasswordBox passBox)
            {
                password = passBox.Password;
            }

            if (string.IsNullOrEmpty(password))
            {
                ErrorMessage = "Vui lòng nhập mật khẩu.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var user = await _authService.LoginAsync(Username, password);

                if (user != null)
                {
                    OnLoginSuccess?.Invoke(user);
                }
                else
                {
                    ErrorMessage = "Sai tên đăng nhập hoặc mật khẩu.";
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}