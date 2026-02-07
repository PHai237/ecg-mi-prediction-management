using MedicalEcgClient.Core;
using MedicalEcgClient.Services;
using MedicalEcgClient.ViewModels;
using MedicalEcgClient.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace MedicalEcgClient
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services { get; private set; }

        private readonly InMemoryLogSink _memorySink = new();

        public App()
        {
            string logFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MedicalEcgClient",
                "Logs");

            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            string logPath = Path.Combine(logFolder, "app-.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.File(logPath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Sink(_memorySink)
                .CreateLogger();

            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            Services = ConfigureServices();
            InitializeComponent();
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ILogger>(Log.Logger);
            services.AddSingleton(_memorySink);

            var settings = AppSettings.Load();
            services.AddSingleton(settings);

            services.AddHttpClient("MedicalApi", client =>
            {
                client.BaseAddress = new Uri(settings.ServerUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddStandardResilienceHandler();

            services.AddSingleton<IAuthService>(sp =>
                new ApiAuthService(
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient("MedicalApi"),
                    sp.GetRequiredService<ILogger>()));

            services.AddSingleton<IPatientService>(sp =>
                new ApiPatientService(
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient("MedicalApi"),
                    sp.GetRequiredService<ILogger>(),
                    sp.GetRequiredService<IAuthService>()
                ));

            services.AddSingleton<ICaseService>(sp =>
                new ApiCaseService(
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient("MedicalApi"),
                    sp.GetRequiredService<ILogger>(),
                    sp.GetRequiredService<IAuthService>()
                ));

            services.AddSingleton<IEcgConnector, UniversalEcgConnector>();
            services.AddSingleton<IImageService, OpenCvImageService>();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<DebuggerViewModel>();

            services.AddTransient<LoginViewModel>();
            services.AddTransient<PatientListViewModel>();
            services.AddTransient<PatientHistoryViewModel>();
            services.AddTransient<EcgMonitorViewModel>();
            services.AddTransient<CameraViewModel>();
            services.AddTransient<SettingsViewModel>();

            services.AddTransient<MainWindow>();
            services.AddTransient<DebuggerWindow>();

            return services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Log.Information("=== SYSTEM STARTED | Path: {AppData} ===",
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = Services.GetRequiredService<MainViewModel>();

            mainWindow.Closed += (s, args) =>
            {
                Log.Information("Main Window Closed. Shutting down application...");
                Application.Current.Shutdown();
            };

            mainWindow.Show();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMsg = BuildCrashReport(e.Exception, "UI_THREAD_CRASH");
            Log.Fatal(e.Exception, errorMsg);

            MessageBox.Show($"LỖI HỆ THỐNG (HOT EXCEPTION):\n{e.Exception.Message}\n\nChi tiết đã ghi vào log (Ctrl+F12).",
                            "CRITICAL ERROR", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true;
        }

        private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                string errorMsg = BuildCrashReport(ex, "BACKGROUND_CRASH");
                Log.Fatal(ex, errorMsg);
            }
        }

        private string BuildCrashReport(Exception ex, string type)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[CRITICAL] {type} DETECTED!");
            sb.AppendLine($"Message: {ex.Message}");
            sb.AppendLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                sb.AppendLine($"Inner Exception: {ex.InnerException.Message}");
            }
            return sb.ToString();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("=== SYSTEM STOPPED ===");
            Log.CloseAndFlush();
            Environment.Exit(0);
            base.OnExit(e);
        }
    }
}