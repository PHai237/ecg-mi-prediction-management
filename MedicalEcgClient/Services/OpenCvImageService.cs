using OpenCvSharp;
using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MedicalEcgClient.Services
{
    public class ImageQualityReport
    {
        public bool IsBlurry { get; set; }
        public double BlurScore { get; set; }
        public bool IsSkewed { get; set; }
        public string Resolution { get; set; } = string.Empty;
        public string Recommendation { get; set; } = "Ảnh đạt chuẩn.";
        public string ColorCode { get; set; } = "LimeGreen";
    }

    public interface IImageService
    {
        Task StartCameraAsync(int cameraIndex, Action<BitmapSource> onFrameReady, Action<string, bool> onQualityChecked);
        void StopCamera();
        void CaptureSnapshot(string patientId, BitmapSource finalImage);
        BitmapSource LoadImageFromFile(string filePath);
        ImageQualityReport AnalyzeImageQuality(BitmapSource image);
        BitmapSource RotateBitmap(BitmapSource source, double angle);
        bool IsRunning { get; }
    }

    public class OpenCvImageService : IImageService, IDisposable
    {
        private VideoCapture? _capture;
        private CancellationTokenSource? _cts;
        private Task? _cameraTask;
        private readonly ILogger _logger;
        private bool _isRunning = false;

        private const double BLUR_THRESHOLD = 100.0;

        public bool IsRunning => _isRunning;

        public OpenCvImageService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task StartCameraAsync(int cameraIndex, Action<BitmapSource> onFrameReady, Action<string, bool> onQualityChecked)
        {
            if (_isRunning) return;

            _capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW);
            if (!_capture.IsOpened()) _capture = new VideoCapture(cameraIndex);

            if (!_capture.IsOpened()) throw new Exception("Không thể mở Camera.");

            _isRunning = true;
            _cts = new CancellationTokenSource();
            _cameraTask = Task.Run(() => CameraLoop(_cts.Token, onFrameReady, onQualityChecked), _cts.Token);
        }

        private void CameraLoop(CancellationToken token, Action<BitmapSource> onFrameReady, Action<string, bool> onQualityChecked)
        {
            using Mat frame = new Mat();
            using Mat gray = new Mat();

            while (!token.IsCancellationRequested && _isRunning)
            {
                try
                {
                    if (_capture != null && _capture.Read(frame) && !frame.Empty())
                    {
                        var bitmap = MatToBitmapSource(frame);
                        onFrameReady?.Invoke(bitmap);

                        if (DateTime.Now.Millisecond % 300 < 50)
                        {
                            Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                            double variance = GetLaplacianVariance(gray);
                            bool isBlurry = variance < BLUR_THRESHOLD;
                            string status = isBlurry ? $"CẢNH BÁO: ẢNH MỜ ({variance:F0})" : $"Tốt ({variance:F0})";
                            onQualityChecked?.Invoke(status, !isBlurry);
                        }
                    }
                    Thread.Sleep(33);
                }
                catch { }
            }
        }

        public void StopCamera()
        {
            _isRunning = false;
            _cts?.Cancel();
            if (_cameraTask != null && _cameraTask.Status == TaskStatus.Running) _cameraTask.Wait(500);
            _capture?.Release();
            _capture?.Dispose();
            _capture = null;
        }

        public BitmapSource LoadImageFromFile(string filePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(filePath);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        public ImageQualityReport AnalyzeImageQuality(BitmapSource image)
        {
            var report = new ImageQualityReport();

            report.Resolution = $"{image.PixelWidth} x {image.PixelHeight}";
            bool lowRes = image.PixelWidth < 800 || image.PixelHeight < 600;

            double variance = 0;
            try
            {
                using var mat = BitmapSourceToMat(image);
                using var gray = new Mat();
                Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);
                variance = GetLaplacianVariance(gray);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error analyzing image blur");
                variance = 500;
            }

            report.BlurScore = variance;
            report.IsBlurry = variance < BLUR_THRESHOLD;

            if (lowRes)
            {
                report.Recommendation = "Độ phân giải thấp. Nên chụp lại hoặc scan lại.";
                report.ColorCode = "Orange";
            }
            else if (report.IsBlurry)
            {
                report.Recommendation = "Ảnh bị mờ/nhòe. Hãy giữ chắc tay hoặc lấy nét lại.";
                report.ColorCode = "Red";
            }
            else
            {
                report.Recommendation = "Chất lượng ảnh Tốt. Có thể lưu.";
                report.ColorCode = "LimeGreen";
            }

            return report;
        }

        public BitmapSource RotateBitmap(BitmapSource source, double angle)
        {
            var cached = new CachedBitmap(source, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            var tb = new TransformedBitmap();
            tb.BeginInit();
            tb.Source = cached;
            tb.Transform = new System.Windows.Media.RotateTransform(angle);
            tb.EndInit();
            tb.Freeze();
            return tb;
        }

        public void CaptureSnapshot(string patientId, BitmapSource finalImage)
        {
            _logger.Information($"Snapshot captured for Patient {patientId}. Res: {finalImage.PixelWidth}x{finalImage.PixelHeight}");
        }

        public void Dispose() => StopCamera();

        private BitmapSource MatToBitmapSource(Mat image)
        {
            using var stream = image.ToMemoryStream(".bmp");
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = stream;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }

        private Mat BitmapSourceToMat(BitmapSource source)
        {
            using var stream = new MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(stream);
            var bytes = stream.ToArray();
            return Cv2.ImDecode(bytes, ImreadModes.Color);
        }

        private double GetLaplacianVariance(Mat src)
        {
            using Mat laplacian = new Mat();
            Cv2.Laplacian(src, laplacian, MatType.CV_64F);
            using Mat mean = new Mat();
            using Mat stddev = new Mat();
            Cv2.MeanStdDev(laplacian, mean, stddev);
            double std = stddev.At<double>(0);
            return std * std;
        }
    }
}