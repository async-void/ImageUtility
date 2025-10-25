using ImageUtility.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Interfaces
{
    public interface IChartBuilder
    {
        (IEnumerable<ISeries> Series, Axis[] XAxes, Axis[] YAxes)
        BuildConverterTotalsChart(UserStatsHistory history);

    }
}
