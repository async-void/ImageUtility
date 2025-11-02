using CommunityToolkit.Mvvm.ComponentModel;
using ImageUtility.Interfaces;
using ImageUtility.ViewModels;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.Avalonia;
using Material.Icons;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using Humanizer;

namespace ImageUtility.Features.Dashboard
{
    public partial class DashboardViewModel : ViewModelBase
    {
        private readonly IJsonData? _jsonDataProviderService;
        private readonly IChartBuilder? _statsChartBuilderService;

        [ObservableProperty]
        private string? _lastUsed;
        [ObservableProperty]
        private string? _lastUpdated;
        [ObservableProperty]
        private int? _convertedCount;
        [ObservableProperty]
        private int? _renamerCount;
        [ObservableProperty]
        private int? _resizerCount;
        [ObservableProperty]
        private int? _convertedSuccessCount;
        [ObservableProperty]
        private int? _renamerSuccessCount;
        [ObservableProperty]
        private int? _resizerSuccessCount;
        [ObservableProperty]
        private int? _failureCount;

        public ObservableCollection<ISeries> Series { get; set; }
        public ObservableCollection<ISeries> RenamerSeries { get; set; }
        public ObservableCollection<ISeries> ResizerSeries { get; set; }
        public ObservableCollection<Axis> XAxes { get; set; }

        public DashboardViewModel(IJsonData jsonProvider, IChartBuilder chartBuilder) : base("Dashboard", MaterialIconKind.ViewDashboard, 1)
        {
           _jsonDataProviderService = jsonProvider;
           _statsChartBuilderService = chartBuilder;

            Task.Run(async () =>
            {
                await LoadStats();
            });
        }

        private async Task LoadStats()
        {
            var result = await _jsonDataProviderService!.LoadStatsAsync();

            if (result.Value is null) return;
            var dates = result.Value.Days?.Select(d => d.Date).ToArray();
            
            LastUsed = dates?.Last().Humanize() ?? DateTime.Now.Humanize();
          
            var conversions = result.Value.Days?.Select(x => x.Stats?.Converter).ToList() ;
            var renamers = result.Value.Days?.Select(x => x.Stats?.Renamer).ToList();
            var resizers = result.Value.Days?.Select(x => x.Stats?.Resizer).ToList();

            ConvertedCount = conversions?.Where(x => x != null).Sum(x => x?.Total) ?? 0;
            RenamerCount = renamers?.Where(x => x != null).Sum(x => x?.Total) ?? 0;
            ResizerCount = resizers?.Where(x => x != null).Sum(x => x?.Total) ?? 0;
            ConvertedSuccessCount = conversions?.Where(x => x != null).Sum(x => x?.Success) ?? 0;
            ResizerSuccessCount = resizers?.Where(x => x != null).Sum(x => x?.Success) ?? 0;
            RenamerSuccessCount = renamers?.Where(x => x != null).Sum(x => x?.Success) ?? 0;
            
            FailureCount = conversions?.Where(x => x != null).Sum(x => x?.Fail) ?? 0;

            Series =
            [
                new LineSeries<ObservableValue>
                {
                    Values = conversions?.Where(x => x != null).Select(x => new ObservableValue(x?.Total)).ToList(),
                    Name = "Conversions",

                },
                new LineSeries<ObservableValue>
                {
                    Values = renamers?.Where(x => x != null).Select(x => new ObservableValue(x?.Total)).ToList(),
                    Name = "Renaming"
                },
                new LineSeries<ObservableValue>
                {
                    Values = resizers?.Where(x => x != null).Select(x => new ObservableValue(x?.Total)).ToList(),
                    Name = "Resizing"
                },
            ];

            XAxes =
            [
                new Axis
                {
                    Labels = dates?.Select(d => d.ToString("MM-dd")).ToArray(),
                    Name = "Date",
                    LabelsRotation = 33
                }
            ];

        }
    }
}
