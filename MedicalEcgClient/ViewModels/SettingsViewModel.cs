using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicalEcgClient.Core;
using System.Collections.ObjectModel;
using System.Windows;

namespace MedicalEcgClient.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        public AppSettings Config { get; }

        public SettingsViewModel(AppSettings config)
        {
            Config = config;
        }

        [RelayCommand]
        public void Save()
        {
            Config.Save();
            MessageBox.Show("Đã lưu cấu hình! Vui lòng khởi động lại ứng dụng để áp dụng đầy đủ các thay đổi phần cứng.", "Đã lưu", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    
    public partial class DebuggerViewModel : ObservableObject
    {
        public AppSettings Config { get; }
        public ObservableCollection<LogEventDisplay> LiveLogs { get; }

        public DebuggerViewModel(AppSettings config, InMemoryLogSink logSink)
        {
            Config = config;
            LiveLogs = logSink.Logs;
        }

        [RelayCommand]
        public void ClearLogs()
        {
            LiveLogs.Clear();
        }

        [RelayCommand]
        public void ForceGC()
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }

        [RelayCommand]
        public void SaveConfigHot()
        {
            Config.Save();
        }
    }
}