using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicalEcgClient.Core;
using MedicalEcgClient.Services;
using ScottPlot;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MedicalEcgClient.ViewModels
{
    public partial class EcgMonitorViewModel : ObservableObject, IDisposable
    {
        private readonly IEcgConnector _ecgConnector;
        private readonly ILogger _logger;
        private readonly IPatientService _patientService;
        private readonly ICaseService _caseService;
        private readonly Dictionary<string, List<double>> _displayBuffers = new();
        public const int MAX_DISPLAY_SAMPLES = 1500;
        private readonly Dictionary<string, List<double>> _recordingBuffers = new();
        private DispatcherTimer? _recordingLimitTimer;
        private DateTime _recordingStartTime;

        public readonly string[] ActiveLeads = { "I", "II", "III", "aVR", "aVL", "aVF" };

        [ObservableProperty] private string _statusMessage = "Sẵn sàng đo";
        [ObservableProperty] private bool _isRecording;
        [ObservableProperty] private bool _isUploading;
        [ObservableProperty] private double _uploadProgress;
        [ObservableProperty] private double _heartRate = 0;
        [ObservableProperty] private string _recordingDuration = "00:00";
        [ObservableProperty] private Patient? _currentPatient;

        public Action? RequestGoBack;
        public Action<Dictionary<string, double[]>>? RequestPlotUpdate;

        public EcgMonitorViewModel(IEcgConnector ecgConnector, ILogger logger, IPatientService patientService, ICaseService caseService)
        {
            _ecgConnector = ecgConnector;
            _logger = logger;
            _patientService = patientService;
            _caseService = caseService;

            _ecgConnector.MultiChannelDataReceived += OnDataReceived;
            _ecgConnector.StatusChanged += OnStatusChanged;

            foreach (var lead in ActiveLeads)
            {
                _displayBuffers[lead] = new List<double>(MAX_DISPLAY_SAMPLES);
                _recordingBuffers[lead] = new List<double>();
            }

            _recordingLimitTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _recordingLimitTimer.Tick += (s, e) =>
            {
                var elapsed = DateTime.Now - _recordingStartTime;
                RecordingDuration = elapsed.ToString(@"mm\:ss");

                if (elapsed.TotalMinutes >= 3)
                {
                    StopRecording();
                    MessageBox.Show("Đã đạt giới hạn thời gian đo (3 phút).", "Tự động dừng", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };
        }

        public void Initialize(Patient patient)
        {
            CurrentPatient = patient;
            StatusMessage = $"Sẵn sàng đo cho BN: {patient.FullName}";

            foreach (var key in _displayBuffers.Keys)
                _displayBuffers[key].Clear();
            foreach (var key in _recordingBuffers.Keys)
                _recordingBuffers[key].Clear();

            HeartRate = 0;
            IsUploading = false;
            UploadProgress = 0;
            RecordingDuration = "00:00";
        }

        [RelayCommand]
        public void ToggleRecording()
        {
            if (IsUploading) return;
            if (IsRecording) StopRecording();
            else StartRecording();
        }

        private void StartRecording()
        {
            foreach (var key in _recordingBuffers.Keys)
                _recordingBuffers[key].Clear();

            _recordingStartTime = DateTime.Now;
            _recordingLimitTimer?.Start();
            _ecgConnector.Start("COM3", 9600);
        }

        private void StopRecording()
        {
            _ecgConnector.Stop();
            _recordingLimitTimer?.Stop();
        }

        private void OnDataReceived(Dictionary<string, double[]> newDataMap)
        {
            if (IsUploading) return;

            lock (_displayBuffers)
            {
                foreach (var lead in ActiveLeads)
                {
                    if (newDataMap.ContainsKey(lead))
                    {
                        var dataChunk = newDataMap[lead];

                        var dispBuf = _displayBuffers[lead];
                        dispBuf.AddRange(dataChunk);
                        if (dispBuf.Count > MAX_DISPLAY_SAMPLES) dispBuf.RemoveRange(0, dispBuf.Count - MAX_DISPLAY_SAMPLES);

                        if (IsRecording)
                        {
                            _recordingBuffers[lead].AddRange(dataChunk);
                        }
                    }
                }
            }

            HeartRate = 75 + (new Random().Next(-2, 2));
            RequestPlotUpdate?.Invoke(newDataMap);
        }

        private byte[] GenerateEcgImage()
        {
            if (_recordingBuffers["I"].Count == 0) return Array.Empty<byte>();

            int totalPoints = _recordingBuffers["I"].Count;

            int width = Math.Max(2000, totalPoints + 100);
            int height = 1080;

            var plot = new ScottPlot.Plot();

            ScottPlot.Color paperBg = ScottPlot.Colors.White;
            ScottPlot.Color gridColor = ScottPlot.Color.FromHex("#FF9999");

            plot.FigureBackground.Color = paperBg;
            plot.DataBackground.Color = paperBg;

            plot.Grid.MajorLineColor = gridColor;
            plot.Grid.MinorLineColor = gridColor.WithOpacity(0.5);
            plot.Grid.IsVisible = true;

            var offsets = new Dictionary<string, double>
            {
                { "I", 10 }, { "II", 6 }, { "III", 2 },
                { "aVR", -2 }, { "aVL", -6 }, { "aVF", -10 }
            };

            foreach (var kvp in offsets)
            {
                string lead = kvp.Key;
                double offset = kvp.Value;

                double[] rawData = _recordingBuffers[lead].ToArray();
                double[] plotData = new double[rawData.Length];

                for (int i = 0; i < rawData.Length; i++)
                {
                    plotData[i] = (rawData[i] * 0.5) + offset;
                }

                var sig = plot.Add.Signal(plotData);
                sig.Color = ScottPlot.Colors.Black;
                sig.LineWidth = 1.0f;

                var txt = plot.Add.Text(lead, 0, offset + 0.5);
                txt.LabelFontColor = ScottPlot.Colors.Black;
                txt.LabelBold = true;
            }

            plot.Axes.SetLimitsX(0, width);
            plot.Axes.SetLimitsY(-14, 14);

            var image = plot.GetImage(width, height);
            return image.GetImageBytes(ScottPlot.ImageFormat.Jpeg, 90);
        }

        [RelayCommand]
        public async Task SaveMeasurement()
        {
            if (IsRecording) StopRecording();

            if (_recordingBuffers["I"].Count == 0)
            {
                MessageBox.Show("Chưa có dữ liệu ghi âm. Vui lòng đo ít nhất vài giây.", "Trống", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CurrentPatient == null) return;

            var confirm = MessageBox.Show($"Lưu kết quả ECG ({RecordingDuration}) cho BN {CurrentPatient.FullName}?",
                                         "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            int? createdCaseId = null;

            try
            {
                IsUploading = true;
                UploadProgress = 0;
                StatusMessage = "Đang tạo ảnh ECG toàn phần...";

                byte[] ecgImageBytes = await Task.Run(() => GenerateEcgImage());

                if (ecgImageBytes.Length == 0) throw new Exception("Lỗi tạo ảnh ECG.");

                UploadProgress = 20;
                StatusMessage = "Đang tạo hồ sơ...";

                var newCase = await _caseService.CreateCaseAsync(CurrentPatient.Id, DateTime.Now, $"Đo điện tim ({RecordingDuration})");
                if (newCase == null) throw new Exception("Lỗi tạo Case API.");
                createdCaseId = newCase.Id;

                UploadProgress = 50;
                StatusMessage = "Đang tải ảnh lên...";

                string fileName = $"ECG_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                bool uploadSuccess = await _caseService.UploadCaseImageAsync(newCase.Id, ecgImageBytes, fileName);

                if (!uploadSuccess)
                {
                    StatusMessage = "Lỗi upload! Đang hoàn tác...";
                    await _caseService.DeleteCaseAsync(newCase.Id);
                    throw new Exception("Upload ảnh thất bại.");
                }

                CurrentPatient.Status = "Đã khám";
                await _patientService.UpdatePatientAsync(CurrentPatient);

                UploadProgress = 100;
                StatusMessage = "Hoàn tất.";

                MessageBox.Show("Lưu kết quả thành công!", "Xong", MessageBoxButton.OK, MessageBoxImage.Information);
                RequestGoBack?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ECG Save Error");
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (IsUploading)
                return;
            if (IsRecording)
                StopRecording();
            RequestGoBack?.Invoke();
        }
        private void OnStatusChanged(DeviceStatus status)
        {
            StatusMessage = $"Trạng thái: {status}";
            IsRecording = status == DeviceStatus.Connected;
        }
        public void Dispose()
        {
            _ecgConnector.Stop();
            _ecgConnector.MultiChannelDataReceived -= OnDataReceived;
            _recordingLimitTimer?.Stop();
        }
    }
}