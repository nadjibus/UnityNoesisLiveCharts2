using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.Noesis;

namespace ChartsExample
{
    public sealed class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly Random _random = new Random();
        private readonly List<ObservableValue> _values = new List<ObservableValue>();
        private bool _isDark = true;
        private bool _isLive = true;
        private int _revision;
        private double _elapsed;

        public DashboardViewModel()
        {
            LineSeries = new ISeries[]
            {
                new LineSeries<ObservableValue> { Name = "Requests", Values = CreateValues(12), Fill = null, GeometrySize = 7 },
                new LineSeries<ObservableValue> { Name = "Responses", Values = CreateValues(12), Fill = null, GeometrySize = 7 }
            };
            ColumnSeries = new ISeries[]
            {
                new ColumnSeries<ObservableValue> { Name = "Desktop", Values = CreateValues(7) },
                new ColumnSeries<ObservableValue> { Name = "Mobile", Values = CreateValues(7) }
            };
            AreaSeries = new ISeries[]
            {
                new LineSeries<ObservableValue> { Name = "Throughput", Values = CreateValues(12), GeometrySize = 0, LineSmoothness = 0.7 }
            };
            PieSeries = CreateSlices(false);
            DoughnutSeries = CreateSlices(true);

            RandomizeCommand = new RelayCommand(Randomize);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            ToggleLiveCommand = new RelayCommand(ToggleLive);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ISeries[] LineSeries { get; }
        public ISeries[] ColumnSeries { get; }
        public ISeries[] AreaSeries { get; }
        public ISeries[] PieSeries { get; }
        public ISeries[] DoughnutSeries { get; }
        public ICommand RandomizeCommand { get; }
        public ICommand ToggleThemeCommand { get; }
        public ICommand ToggleLiveCommand { get; }
        public bool IsDark => _isDark;
        public bool IsLive => _isLive;
        public int Revision => _revision;
        public string ThemeLabel => _isDark ? "☀  Light theme" : "☾  Dark theme";
        public string LiveLabel => _isLive ? "Ⅱ  Pause updates" : "▷  Resume updates";
        public string UpdateLabel => "DATA UPDATE " + _revision.ToString("D3");

        public void Tick(double deltaTime)
        {
            if (!_isLive)
            {
                return;
            }

            _elapsed += deltaTime;
            if (_elapsed >= 3)
            {
                _elapsed = 0;
                Randomize();
            }
        }

        private ObservableValue[] CreateValues(int count)
        {
            var values = new ObservableValue[count];
            for (var index = 0; index < count; index++)
            {
                var value = new ObservableValue(_random.Next(20, 95));
                values[index] = value;
                _values.Add(value);
            }

            return values;
        }

        private ISeries[] CreateSlices(bool doughnut)
        {
            var names = new[] { "Desktop", "Mobile", "Console", "Other" };
            var slices = new ISeries[names.Length];
            for (var index = 0; index < names.Length; index++)
            {
                slices[index] = new PieSeries<ObservableValue>
                {
                    Name = names[index],
                    Values = CreateValues(1),
                    InnerRadius = doughnut ? 65 : 0,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    DataLabelsSize = 16
                };
            }

            return slices;
        }

        private void Randomize()
        {
            foreach (var value in _values)
            {
                value.Value = _random.Next(20, 95);
            }

            _revision++;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpdateLabel)));
        }

        private void ToggleTheme()
        {
            _isDark = !_isDark;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDark)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ThemeLabel)));
        }

        private void ToggleLive()
        {
            _isLive = !_isLive;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLive)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LiveLabel)));
        }
    }
}
