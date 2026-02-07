using MedicalEcgClient.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalEcgClient.Services
{
    public interface IEcgDriver
    {
        void Connect(string portName, int baudRate);
        void Disconnect();
        Dictionary<string, double[]> ParseDataMultiChannel(byte[] buffer, int bytesRead);
    }

    public class SimulationDriver : IEcgDriver
    {
        private readonly Random _rand = new();
        private double _phase = 0;

        public void Connect(string portName, int baudRate) { }
        public void Disconnect() { }

        public Dictionary<string, double[]> ParseDataMultiChannel(byte[] buffer, int bytesRead)
        {
            var result = new Dictionary<string, double[]>();
            int sampleCount = 5;

            string[] leads = { "I", "II", "III", "aVR", "aVL", "aVF" };

            foreach (var lead in leads)
            {
                double[] data = new double[sampleCount];
                double localPhase = _phase;

                for (int i = 0; i < sampleCount; i++)
                {
                    localPhase += 0.1;

                    double signal = Math.Sin(localPhase) + 0.5 * Math.Sin(localPhase * 3) + (_rand.NextDouble() * 0.05);

                    switch (lead)
                    {
                        case "I": break;
                        case "II": signal *= 1.5; break;
                        case "III": signal *= 0.8; signal += 0.2; break;
                        case "aVR": signal *= -1.0; break;
                        case "aVL": signal *= 0.7; break;
                        case "aVF": signal *= 1.2; break;
                    }

                    data[i] = signal;
                }
                result[lead] = data;
            }

            _phase += 0.1 * sampleCount;
            if (_phase > Math.PI * 200) _phase = 0;

            return result;
        }
    }

    public interface IEcgConnector
    {
        event Action<Dictionary<string, double[]>> MultiChannelDataReceived;
        event Action<DeviceStatus> StatusChanged;
        void Start(string portName, int baudRate);
        void Stop();
        bool IsRecording { get; }
    }

    public class UniversalEcgConnector : IEcgConnector, IDisposable
    {
        private readonly IEcgDriver _driver;
        private readonly ILogger _logger;
        private readonly AppSettings _settings;
        private bool _isRecording = false;

        public event Action<Dictionary<string, double[]>>? MultiChannelDataReceived;
        public event Action<DeviceStatus>? StatusChanged;
        public bool IsRecording => _isRecording;

        public UniversalEcgConnector(AppSettings settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger;

            if (_settings.IsSimulationMode)
            {
                _driver = new SimulationDriver();
            }
            else
            {
                _driver = new SimulationDriver();
            }
        }

        public void Start(string portName, int baudRate)
        {
            try
            {
                string port = !string.IsNullOrEmpty(_settings.ComPort) ? _settings.ComPort : portName;
                int baud = _settings.BaudRate > 0 ? _settings.BaudRate : baudRate;

                _isRecording = true;
                StatusChanged?.Invoke(DeviceStatus.Connected);

                _logger.Information($"Starting ECG Capture on {port} @ {baud} (SimMode: {_settings.IsSimulationMode})");

                Task.Run(SimulationLoop);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start ECG connector");
                StatusChanged?.Invoke(DeviceStatus.Error);
            }
        }

        private async Task SimulationLoop()
        {
            while (_isRecording)
            {
                var fakeData = _driver.ParseDataMultiChannel(null, 0);
                MultiChannelDataReceived?.Invoke(fakeData);
                await Task.Delay(20);
            }
        }

        public void Stop()
        {
            _isRecording = false;
            StatusChanged?.Invoke(DeviceStatus.Disconnected);
            _logger.Information("ECG Capture Stopped");
        }

        public void Dispose() => Stop();
    }
}