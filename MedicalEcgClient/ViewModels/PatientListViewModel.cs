using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicalEcgClient.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MedicalEcgClient.ViewModels
{
    public partial class PatientListViewModel : ObservableObject
    {
        private readonly IPatientService _patientService;
        private readonly Serilog.ILogger _logger;
        private List<Patient> _allPatients = new();

        [ObservableProperty]
        private ObservableCollection<Patient> _patients = new();

        [ObservableProperty]
        private Patient? _selectedPatient;

        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private string _editorTitle = "Thêm Bệnh Nhân Mới";

        [ObservableProperty]
        private Patient _editingPatient = new();

        public Action<Patient>? RequestEcgMeasurement;
        public Action<Patient>? RequestCameraCapture;
        public Action<Patient>? RequestOpenPatientHistory;

        public PatientListViewModel(IPatientService patientService, Serilog.ILogger logger)
        {
            _patientService = patientService;
            _logger = logger;
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            _logger.Debug("Loading patient list...");
            _allPatients = await _patientService.GetDailyPatientsAsync("current-doc");
            FilterPatients();
            IsLoading = false;
        }

        partial void OnSearchKeywordChanged(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > 2)
                _logger.Information("[USER-ACTION] Searching for: '{Keyword}'", value);
            FilterPatients();
        }

        private void FilterPatients()
        {
            if (string.IsNullOrWhiteSpace(SearchKeyword))
            {
                Patients = new ObservableCollection<Patient>(_allPatients);
            }
            else
            {
                var filtered = _allPatients.Where(p =>
                    p.FullName.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase) ||
                    p.PatientCode.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase));
                Patients = new ObservableCollection<Patient>(filtered);
            }
        }

        [RelayCommand]
        public void OpenAddPatient()
        {
            _logger.Information("[USER-ACTION] Opened 'Add Patient' Form");
            IsEditMode = false;
            EditorTitle = "TIẾP NHẬN BỆNH NHÂN MỚI";

            EditingPatient = new Patient
            {
                PatientCode = $"BN{DateTime.Now:yyMMddHHmm}",
                DateOfBirth = new DateTime(1990, 1, 1),
                Note = "",
                Status = "Waiting"
            };
            IsEditing = true;
        }

        [RelayCommand]
        public void OpenEditPatient()
        {
            if (SelectedPatient == null) return;

            IsEditMode = true;
            EditorTitle = "CẬP NHẬT THÔNG TIN / XÓA";

            EditingPatient = new Patient
            {
                Id = SelectedPatient.Id,
                PatientCode = SelectedPatient.PatientCode,
                FullName = SelectedPatient.FullName,
                DateOfBirth = SelectedPatient.DateOfBirth,
                Gender = SelectedPatient.Gender,
                Note = SelectedPatient.Note,
                Status = SelectedPatient.Status
            };
            IsEditing = true;
        }

        [RelayCommand]
        public async Task SavePatient()
        {
            if (string.IsNullOrWhiteSpace(EditingPatient.FullName))
            {
                MessageBox.Show("Vui lòng nhập họ tên bệnh nhân.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            _logger.Information("[USER-ACTION] Saving Patient... (EditMode: {Mode})", IsEditMode);

            if (IsEditMode)
                await _patientService.UpdatePatientAsync(EditingPatient);
            else
                await _patientService.CreatePatientAsync(EditingPatient);

            await LoadDataAsync();
            IsEditing = false;
            IsLoading = false;
        }

        [RelayCommand]
        public async Task DeletePatient()
        {
            if (!IsEditMode) return;

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa bệnh nhân '{EditingPatient.FullName}' không?\nHành động này không thể hoàn tác.",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _logger.Warning("[AUDIT] User CONFIRMED deletion of patient: {Name} ({Code})", EditingPatient.FullName, EditingPatient.PatientCode);

                IsLoading = true;
                await _patientService.DeletePatientAsync(EditingPatient.Id);
                await LoadDataAsync();

                IsEditing = false;
                IsLoading = false;
            }
            else
            {
                _logger.Information("[USER-ACTION] User CANCELLED deletion.");
            }
        }

        [RelayCommand]
        public void CancelEdit()
        {
            _logger.Information("[USER-ACTION] Cancelled Edit/Add Form");
            IsEditing = false;
        }

        [RelayCommand]
        public void GoToEcg()
        {
            if (SelectedPatient != null)
            {
                _logger.Information("[USER-ACTION] Open History for: {Name}", SelectedPatient.FullName);
                RequestOpenPatientHistory?.Invoke(SelectedPatient);
            }
        }

        [RelayCommand]
        public async Task Refresh()
        {
            _logger.Information("[USER-ACTION] User clicked Refresh");
            await LoadDataAsync();
        }
    }
}