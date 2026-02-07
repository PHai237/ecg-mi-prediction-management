using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicalEcgClient.Core;
using MedicalEcgClient.Services;
using Microsoft.Win32;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MedicalEcgClient.ViewModels
{
    public partial class CameraViewModel : ObservableObject
    {
        private readonly IImageService _imageService;
        private readonly ICaseService _caseService;
        private readonly IPatientService _patientService;
        private readonly ILogger _logger;

        [ObservableProperty] private Patient? _currentPatient;
        [ObservableProperty] private BitmapSource? _currentFrame;
        [ObservableProperty] private bool _isCameraRunning;
        [ObservableProperty] private bool _isInitializing;
        [ObservableProperty] private string _errorMessage = string.Empty;
        [ObservableProperty] private bool _isStaticImageMode;
        [ObservableProperty] private bool _isCapturedFromCamera;
        [ObservableProperty] private string _qualityStatus = "Chờ tín hiệu...";
        [ObservableProperty] private string _qualityColor = "Gray";
        [ObservableProperty] private string _recommendation = "";
        [ObservableProperty] private string _resolutionInfo = "";
        [ObservableProperty] private bool _showAnalysisPanel = false;

        [ObservableProperty] private bool _isUploading;
        [ObservableProperty] private double _uploadProgress;
        [ObservableProperty] private string _statusMessage = "Sẵn sàng.";

        public Action? RequestGoBack;

        public CameraViewModel(IImageService imageService, ICaseService caseService, IPatientService patientService, ILogger logger)
        {
            _imageService = imageService;
            _caseService = caseService;
            _patientService = patientService;
            _logger = logger;
        }

        public void Initialize(Patient patient)
        {
            CurrentPatient = patient;
            ErrorMessage = "";
            IsStaticImageMode = false;
            CurrentFrame = null;
            IsCameraRunning = false;
            IsInitializing = false;
            ShowAnalysisPanel = false;
            IsUploading = false;
            UploadProgress = 0;
            StatusMessage = "Sẵn sàng.";
            IsCapturedFromCamera = false;
        }
        [RelayCommand]
        public void BrowseImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                if (IsCameraRunning)
                {
                    _imageService.StopCamera();
                    IsCameraRunning = false;
                }
                try
                {
                    var image = _imageService.LoadImageFromFile(openFileDialog.FileName);
                    IsCapturedFromCamera = false; SetImageAndAnalyze(image);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi đọc file: {ex.Message}");
                }
            }
        }
        private void SetImageAndAnalyze(BitmapSource image)
        {
            CurrentFrame = image;
            IsStaticImageMode = true;
            ErrorMessage = "";
            var report = _imageService.AnalyzeImageQuality(image);
            QualityStatus = report.IsBlurry ? "ẢNH MỜ" : "ẢNH NÉT";
            QualityColor = report.ColorCode;
            Recommendation = report.Recommendation;
            ResolutionInfo = $"Kích thước: {report.Resolution}";
            ShowAnalysisPanel = true;
        }
        [RelayCommand]
        public void RotateLeft() => SetImageAndAnalyze(_imageService.RotateBitmap(CurrentFrame!, -90));
        [RelayCommand]
        public void RotateRight() => SetImageAndAnalyze(_imageService.RotateBitmap(CurrentFrame!, 90));
        [RelayCommand]
        public void Retake() => _ = StartCameraSafe();
        private async Task StartCameraSafe()
        {
            if (IsCameraRunning || IsInitializing || IsUploading)
                return; IsInitializing = true; IsStaticImageMode = false;
            ShowAnalysisPanel = false;
            try 
            { 
                await _imageService.StartCameraAsync(0, OnFrameReceived, OnQualityChecked); 
                IsCameraRunning = true; 
            } 
            catch (Exception ex) 
            { 
                ErrorMessage = ex.Message; 
                IsCameraRunning = false; 
            } 
            finally 
            { 
                IsInitializing = false; 
            }
        }
        [RelayCommand] 
        public void ToggleCamera() 
        { 
            if (IsCameraRunning) 
            { 
                _imageService.StopCamera(); 
                IsCameraRunning = false; 
                if (CurrentFrame != null) 
                { 
                    var f = CurrentFrame.Clone(); 
                    f.Freeze(); 
                    IsCapturedFromCamera = true; 
                    SetImageAndAnalyze(f); 
                } 
            } 
            else 
            { 
                _ = StartCameraSafe(); 
            }
        }
        private void OnFrameReceived(BitmapSource frame) 
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                if (IsCameraRunning && !IsStaticImageMode) 
                { 
                    CurrentFrame = frame; 
                } 
            }); 
        }
        private void OnQualityChecked(string msg, bool ok) 
        {
            if (!IsStaticImageMode) 
            Application.Current.Dispatcher.Invoke(() => 
            { 
                QualityStatus = msg; 
                QualityColor = ok ? "LimeGreen" : "Red"; 
            }); 
        }

        [RelayCommand]
        public async Task Capture()
        {
            if (CurrentFrame == null || CurrentPatient == null) return;
            if (IsUploading) return;

            if (IsCameraRunning)
            {
                _imageService.StopCamera();
                IsCameraRunning = false;
                if (!IsStaticImageMode && CurrentFrame != null)
                {
                    var staticFrame = CurrentFrame.Clone();
                    staticFrame.Freeze();
                    IsCapturedFromCamera = true;
                    SetImageAndAnalyze(staticFrame);
                }
                return;
            }

            var safePatient = CurrentPatient;
            var safeFrame = CurrentFrame;

            if (safePatient == null || safeFrame == null) return;

            var confirm = MessageBox.Show($"Lưu hình ảnh vào hồ sơ BN {safePatient.FullName}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            int? createdCaseId = null;

            try
            {
                IsUploading = true;
                UploadProgress = 0;
                StatusMessage = "Đang tạo ca khám mới...";

                var newCase = await _caseService.CreateCaseAsync(safePatient.Id, DateTime.Now, "Chụp ảnh hồ sơ/tài liệu");
                if (newCase == null) throw new Exception("Không thể tạo ca khám mới (API Error).");
                createdCaseId = newCase.Id;

                UploadProgress = 40;
                StatusMessage = $"Đang tải ảnh lên (Case #{newCase.Id})...";

                string fileName = $"IMG_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                bool uploadSuccess = await _caseService.UploadCaseImageAsync(newCase.Id, safeFrame, fileName);

                if (!uploadSuccess)
                {
                    StatusMessage = "Upload lỗi! Đang hủy ca khám...";
                    _logger.Warning($"Upload failed. Rollback Case {createdCaseId}");
                    await _caseService.DeleteCaseAsync(newCase.Id);
                    throw new Exception("Tải ảnh lên thất bại. Hệ thống đã hủy ca khám này.");
                }

                UploadProgress = 90;
                StatusMessage = "Cập nhật trạng thái bệnh nhân...";

                safePatient.Status = "Đã khám";
                await _patientService.UpdatePatientAsync(safePatient);

                UploadProgress = 100;
                StatusMessage = "Hoàn tất.";

                _logger.Information($"Full workflow complete: Case {newCase.Id} created + Image uploaded + Patient status updated.");

                MessageBox.Show("Đã lưu và cập nhật trạng thái 'Đã khám'!", "Hoàn tất", MessageBoxButton.OK, MessageBoxImage.Information);
                RequestGoBack?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Capture Workflow Failed");
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsUploading = false;
                StatusMessage = "Sẵn sàng.";
            }
        }

        [RelayCommand]
        public void GoBack()
        {
            if (IsUploading) return;
            if (IsCameraRunning) _imageService.StopCamera();
            RequestGoBack?.Invoke();
        }

        public void Dispose()
        {
            if (IsCameraRunning) _imageService.StopCamera();
        }
    }
}