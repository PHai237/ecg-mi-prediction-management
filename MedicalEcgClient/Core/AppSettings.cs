using CommunityToolkit.Mvvm.ComponentModel;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

namespace MedicalEcgClient.Core
{
    public class AppSettings : ObservableObject
    {
        private static readonly string _appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MedicalEcgClient");
        private static readonly string _configFilePath = Path.Combine(_appDataFolder, "settings.json");
        private string _serverUrl = "https://apimed.thanglele.cloud";
        public string ServerUrl { get => _serverUrl; set => SetProperty(ref _serverUrl, value); }
        private string _comPort = "COM3";
        public string ComPort { get => _comPort; set => SetProperty(ref _comPort, value); }
        private int _baudRate = 9600;
        public int BaudRate { get => _baudRate; set => SetProperty(ref _baudRate, value); }
        private bool _isSimulationMode = true;
        public bool IsSimulationMode { get => _isSimulationMode; set => SetProperty(ref _isSimulationMode, value); }
        private bool _enableRawLogging = false;
        public bool EnableRawLogging { get => _enableRawLogging; set => SetProperty(ref _enableRawLogging, value); }

        public void Save()
        {
            try
            {
                if (!Directory.Exists(_appDataFolder))
                {
                    Directory.CreateDirectory(_appDataFolder);
                }

                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configFilePath, json);
            }
            catch { /* Ignore config save error */ }
        }

        public static AppSettings Load()
        {
            if (!File.Exists(_configFilePath)) return new AppSettings();
            try
            {
                var json = File.ReadAllText(_configFilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch { return new AppSettings(); }
        }
    }

    public class InMemoryLogSink : ILogEventSink
    {
        public ObservableCollection<LogEventDisplay> Logs { get; } = new();
        private readonly Dispatcher _uiDispatcher;

        public InMemoryLogSink()
        {
            _uiDispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
        }

        public void Emit(LogEvent logEvent)
        {
            _uiDispatcher.BeginInvoke(() =>
            {
                if (Logs.Count > 1000) Logs.RemoveAt(0);

                string message = logEvent.RenderMessage();
                if (logEvent.Exception != null)
                {
                    message += $"\n[STACK TRACE] {logEvent.Exception.Message}";
                }

                Logs.Add(new LogEventDisplay
                {
                    Timestamp = logEvent.Timestamp.LocalDateTime.ToString("HH:mm:ss.fff"),
                    Level = logEvent.Level.ToString().ToUpper(),
                    Message = message,
                    Color = GetColor(logEvent.Level, message)
                });
            });
        }

        private string GetColor(LogEventLevel level, string message)
        {
            if (message.Contains("[AUDIT]")) return "#00FFFF";
            if (message.Contains("[USER-ACTION]")) return "#00FF00";
            if (message.Contains("[CRITICAL]")) return "#FF00FF";
            if (message.Contains("[METRIC]")) return "#FFFF00";

            return level switch
            {
                LogEventLevel.Fatal => "#FF3333",
                LogEventLevel.Error => "#FF5555",
                LogEventLevel.Warning => "#FFAA00",
                LogEventLevel.Information => "#DDDDDD",
                LogEventLevel.Debug => "#888888",
                _ => "White"
            };
        }
    }

    public class LogEventDisplay
    {
        public string Timestamp { get; set; } = "";
        public string Level { get; set; } = "";
        public string Message { get; set; } = "";
        public string Color { get; set; } = "White";
    }
}