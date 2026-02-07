using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicalEcgClient.Core;
using MedicalEcgClient.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MedicalEcgClient.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthService _authService;
        private readonly AppSettings _appSettings;

        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private Patient? _activePatient;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private string _appTitle = "HỆ THỐNG ECG EDGE CLIENT";

        [ObservableProperty]
        private bool _isLoggedIn = false;

        [ObservableProperty]
        private bool _isBusy = false;

        [ObservableProperty]
        private string _busyMessage = "Đang xử lý...";

        public MainViewModel(IServiceProvider serviceProvider, IAuthService authService, AppSettings appSettings)
        {
            _serviceProvider = serviceProvider;
            _authService = authService;
            _appSettings = appSettings;

            AppTitle = $"HỆ THỐNG ECG EDGE CLIENT v1.0 | Mode: {(_appSettings.IsSimulationMode ? "SIMULATION" : "REAL")}";

            NavigateToLogin();
        }

        private void NavigateToLogin()
        {
            var loginVm = _serviceProvider.GetRequiredService<LoginViewModel>();
            loginVm.OnLoginSuccess = (user) => { CurrentUser = user; IsLoggedIn = true; NavigateToDashboard(); };
            CurrentView = loginVm;
            IsLoggedIn = false;
        }

        [RelayCommand]
        public void NavigateToDashboard()
        {
            if (!IsLoggedIn) return;
            var vm = _serviceProvider.GetRequiredService<PatientListViewModel>();

            vm.RequestOpenPatientHistory = (p) => NavigateToHistory(p);

            vm.RequestEcgMeasurement = (p) => NavigateToHistory(p);
            vm.RequestCameraCapture = (p) => NavigateToHistory(p);

            _ = vm.LoadDataAsync();
            CurrentView = vm;
            ActivePatient = null;
            AppTitle = $"DANH SÁCH BỆNH NHÂN | BS. {CurrentUser?.FullName ?? "N/A"}";
        }

        public void NavigateToHistory(Patient patient)
        {
            ActivePatient = patient;
            var vm = _serviceProvider.GetRequiredService<PatientHistoryViewModel>();

            vm.RequestEcgMeasurement = (p) => NavigateToEcg(p);
            vm.RequestCameraCapture = async (p) => await NavigateToCameraAsync(p);

            vm.RequestGoBack = () => NavigateToDashboard();

            _ = vm.InitializeAsync(patient);

            CurrentView = vm;
            AppTitle = $"HỒ SƠ BỆNH ÁN: {patient.FullName} ({patient.PatientCode})";
        }

        public void NavigateToEcg(Patient patient)
        {
            ActivePatient = patient;
            var vm = _serviceProvider.GetRequiredService<EcgMonitorViewModel>();
            vm.Initialize(patient);

            vm.RequestGoBack = () => NavigateToHistory(patient);

            CurrentView = vm;
            AppTitle = $"ĐO ECG: {patient.FullName}";
        }

        public async Task NavigateToCameraAsync(Patient patient)
        {
            IsBusy = true;
            try
            {
                await Task.Delay(50);
                ActivePatient = patient;
                var vm = _serviceProvider.GetRequiredService<CameraViewModel>();
                vm.Initialize(patient);

                vm.RequestGoBack = () => NavigateToHistory(patient);

                CurrentView = vm;
                AppTitle = $"CHỤP ẢNH: {patient.FullName}";
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        public void Logout()
        {
            if (MessageBox.Show("Đăng xuất?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _authService.Logout();
                IsLoggedIn = false;
                NavigateToLogin();
            }
        }

        [RelayCommand]
        public void NavigateToSettings()
        {
            var settingsVm = _serviceProvider.GetRequiredService<SettingsViewModel>();
            CurrentView = settingsVm;
            ActivePatient = null;
        }

        [RelayCommand]
        public void OpenDebugger()
        {
            foreach (Window w in Application.Current.Windows)
            {
                if (w is DebuggerWindow) { w.Activate(); return; }
            }

            var debugWin = _serviceProvider.GetRequiredService<DebuggerWindow>();
            debugWin.DataContext = _serviceProvider.GetRequiredService<DebuggerViewModel>();
            debugWin.Show();
        }
    }
}