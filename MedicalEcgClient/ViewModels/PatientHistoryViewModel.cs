using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicalEcgClient.Core;
using MedicalEcgClient.Core.Dto;
using MedicalEcgClient.Services;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace MedicalEcgClient.ViewModels
{
    public partial class PatientHistoryViewModel : ObservableObject
    {
        private readonly ICaseService _caseService;
        private readonly ILogger _logger;

        [ObservableProperty]
        private Patient? _currentPatient;

        [ObservableProperty]
        private ObservableCollection<CaseDto> _cases = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private CaseDto? _selectedCase;

        [ObservableProperty]
        private bool _isViewingDetail;

        [ObservableProperty]
        private CaseDto? _currentCaseDetail;

        public Action<Patient>? RequestEcgMeasurement;
        public Action<Patient>? RequestCameraCapture;
        public Action? RequestGoBack;

        public PatientHistoryViewModel(ICaseService caseService, ILogger logger)
        {
            _caseService = caseService;
            _logger = logger;
        }

        public async Task InitializeAsync(Patient patient)
        {
            CurrentPatient = patient;
            Cases.Clear();
            IsViewingDetail = false;
            IsLoading = true;
            try
            {
                _logger.Debug($"Fetching history for Patient ID: {patient.Id}");
                var history = await _caseService.GetHistoryAsync(patient.Id);

                foreach (var item in history)
                {
                    Cases.Add(item);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task ViewCaseDetail()
        {
            if (SelectedCase == null) return;

            IsLoading = true;
            try
            {
                _logger.Information($"[USER-ACTION] Viewing details for Case {SelectedCase.Id}");

                var detail = await _caseService.GetCaseDetailAsync(SelectedCase.Id);

                if (detail != null)
                {
                    CurrentCaseDetail = detail;
                    IsViewingDetail = true;
                }
                else
                {
                    MessageBox.Show("Không thể tải chi tiết ca khám này.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void CloseDetail()
        {
            IsViewingDetail = false;
            CurrentCaseDetail = null;
        }

        [RelayCommand]
        public void CreateEcgCase()
        {
            if (CurrentPatient != null)
                RequestEcgMeasurement?.Invoke(CurrentPatient);
        }

        [RelayCommand]
        public void CreateImageCase()
        {
            if (CurrentPatient != null)
                RequestCameraCapture?.Invoke(CurrentPatient);
        }

        [RelayCommand]
        public void GoBack() => RequestGoBack?.Invoke();
    }
}