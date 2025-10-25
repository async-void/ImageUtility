using ImageUtility.Interfaces;
using ImageUtility.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Services
{
    public class StatsChartBuilderService : IChartBuilder
    {
        public (IEnumerable<ISeries> Series, Axis[] XAxes, Axis[] YAxes) BuildConverterTotalsChart(UserStatsHistory history)
        {
            var dates = history.Days
            .OrderBy(d => d.Date)
            .Select(d => d.Date)
            .ToArray();

            var totals = history.Days
                .OrderBy(d => d.Date)
                .Select(d => d.Stats.Converter.Total)
                .ToArray();
            var t = history.Days[0].Stats.Converter.Total;
            var series = new ISeries[]
            {
            new LineSeries<int>
            {
                Values = [t],
                Name = "Converter Total"
            }
            };

            var xAxes = new[]
            {
            new Axis
            {
                Labels = [dates.FirstOrDefault().ToShortDateString()],
                LabelsRotation = 45
            }
        };

            var yAxes = new[]
            {
            new Axis
            {
                Name = "Total"
            }
        };

            return (series, xAxes, yAxes);

        }
    }
}
