using System;

namespace MedicalEcgClient.Core
{
    public class AppConfig
    {
        public string ServerUrl { get; set; } = "https://apimed.thanglele.cloud";
        public string ComPort { get; set; } = "COM3";
        public int BaudRate { get; set; } = 9600;
        public bool EnableRawLogging { get; set; } = false;
        public bool IsSimulationMode { get; set; } = true;
    }

    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Doctor";
        public string Token { get; set; } = string.Empty;

        public string StaffCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public class Patient
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PatientCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; } = DateTime.Now;

        public string Gender { get; set; } = "Nam";
        public string Note { get; set; } = string.Empty;
        public string Status { get; set; } = "Waiting";
    }

    public enum DeviceStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Error
    }

    public interface IEcgDriver
    {
        void Connect(string portName, int baudRate);
        void Disconnect();
        double[] ParseData(byte[] buffer, int bytesRead);
    }
}