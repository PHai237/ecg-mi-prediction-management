using MedicalEcgClient.ViewModels;
using ScottPlot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MedicalEcgClient.Views
{
    public partial class EcgMonitorView : UserControl
    {
        private EcgMonitorViewModel? _viewModel;
        private DispatcherTimer _renderTimer;
        private ConcurrentDictionary<string, ConcurrentQueue<double>> _incomingDataQueues = new();
        private Dictionary<string, ScottPlot.Plottables.Signal> _signals = new();
        private Dictionary<string, double[]> _renderArrays = new();

        private readonly Dictionary<string, double> _leadOffsets = new()
        {
            { "I",   10 },
            { "II",  6 },
            { "III", 2 },
            { "aVR", -2 },
            { "aVL", -6 },
            { "aVF", -10 }
        };

        public EcgMonitorView()
        {
            InitializeComponent();
            SetupMedicalChart();

            foreach (var key in _leadOffsets.Keys)
            {
                _incomingDataQueues[key] = new ConcurrentQueue<double>();
            }

            _renderTimer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(33)
            };
            _renderTimer.Tick += RenderTimer_Tick;

            this.DataContextChanged += OnDataContextChanged;
            this.Unloaded += (s, e) => _renderTimer.Stop();
        }

        private void SetupMedicalChart()
        {
            var plot = EcgPlot.Plot;

            ScottPlot.Color paperBg = ScottPlot.Colors.White;
            plot.FigureBackground.Color = paperBg;
            plot.DataBackground.Color = paperBg;

            ScottPlot.Color gridColor = ScottPlot.Color.FromHex("#FF9999");

            plot.Grid.MajorLineColor = gridColor;
            plot.Grid.MajorLineWidth = 1.0f;
            plot.Grid.MinorLineColor = gridColor.WithOpacity(0.5);
            plot.Grid.MinorLineWidth = 0.5f;
            plot.Grid.IsVisible = true;

            plot.Axes.Left.TickLabelStyle.IsVisible = false;
            plot.Axes.Bottom.TickLabelStyle.IsVisible = false;

            foreach (var axis in plot.Axes.GetAxes())
            {
                axis.FrameLineStyle.Color = gridColor;
            }

            _signals.Clear();
            _renderArrays.Clear();
            ScottPlot.Color signalColor = ScottPlot.Colors.Black;

            foreach (var kvp in _leadOffsets)
            {
                string leadName = kvp.Key;
                double offset = kvp.Value;

                double[] buffer = new double[EcgMonitorViewModel.MAX_DISPLAY_SAMPLES];
                Array.Fill(buffer, offset);

                _renderArrays[leadName] = buffer;

                var signal = plot.Add.Signal(buffer);
                signal.Color = signalColor;
                signal.LineWidth = 1.5f;
                signal.MarkerSize = 0;

                _signals[leadName] = signal;

                var text = plot.Add.Text(leadName, EcgMonitorViewModel.MAX_DISPLAY_SAMPLES, offset + 0.5);
                text.LabelFontSize = 12;
                text.LabelFontColor = ScottPlot.Colors.Black;
                text.LabelBold = true;
                text.LabelBackgroundColor = paperBg.WithOpacity(0.7);
                text.LabelAlignment = ScottPlot.Alignment.MiddleRight;
            }

            plot.Axes.SetLimitsX(0, EcgMonitorViewModel.MAX_DISPLAY_SAMPLES);
            plot.Axes.SetLimitsY(-12, 12);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is EcgMonitorViewModel vm)
            {
                _viewModel = vm;
                _viewModel.RequestPlotUpdate = OnDataReceivedIntoBuffer;
                _renderTimer.Start();
            }
            else
            {
                _renderTimer.Stop();
            }
        }

        private void OnDataReceivedIntoBuffer(Dictionary<string, double[]> newDataChunk)
        {
            foreach (var kvp in newDataChunk)
            {
                if (_incomingDataQueues.TryGetValue(kvp.Key, out var queue))
                {
                    foreach (var val in kvp.Value)
                    {
                        queue.Enqueue(val);
                    }
                }
            }
        }

        private void RenderTimer_Tick(object? sender, EventArgs e)
        {
            bool needRefresh = false;

            foreach (var kvp in _renderArrays)
            {
                string leadName = kvp.Key;
                double[] renderBuffer = kvp.Value;
                double offset = _leadOffsets[leadName];

                if (_incomingDataQueues.TryGetValue(leadName, out var queue) && !queue.IsEmpty)
                {
                    var incomingBatch = new List<double>();
                    while (queue.TryDequeue(out double val))
                    {
                        double scaledVal = val * 0.5;
                        incomingBatch.Add(scaledVal + offset);
                    }

                    int totalNewPoints = incomingBatch.Count;
                    if (totalNewPoints > 0)
                    {
                        int pointsToDraw = Math.Min(totalNewPoints, renderBuffer.Length);
                        int sourceStartIndex = totalNewPoints - pointsToDraw;

                        int shiftAmount = pointsToDraw;
                        int remaining = renderBuffer.Length - shiftAmount;

                        if (remaining > 0)
                        {
                            Array.Copy(renderBuffer, shiftAmount, renderBuffer, 0, remaining);
                        }

                        for (int i = 0; i < shiftAmount; i++)
                        {
                            renderBuffer[remaining + i] = incomingBatch[sourceStartIndex + i];
                        }

                        needRefresh = true;
                    }
                }
            }

            if (needRefresh)
            {
                EcgPlot.Refresh();
            }
        }
    }
}