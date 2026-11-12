using SmartPalm.AgronomyService.Domain.Services;
using Xunit;

namespace SmartPalm.AgronomyService.API.Tests.Domain;

public class DiagnosisSummaryTests
{
    [Fact]
    public void SummarizesMetricsAndKeepsLightUnitsSeparate()
    {
        var from = DateTimeOffset.Parse("2026-06-18T00:00:00Z");
        var to = from.AddDays(1);
        var result = DiagnosisSummary.Create(from, to, [
            new("temperature", 27, "C", from.AddHours(1)),
            new("temperature", 29, "C", from.AddHours(2)),
            new("light", 66, "lux", from.AddHours(1)),
            new("light", 60, "%", from.AddHours(2))
        ]);
        var temperature = Assert.Single(result.Metrics, x => x.Variable == "temperature");
        Assert.Equal(28, temperature.Average);
        Assert.Equal(27, temperature.Minimum);
        Assert.Equal(29, temperature.Maximum);
        Assert.Equal(2, result.Metrics.Count(x => x.Variable == "light"));
    }

    [Fact]
    public void ExcludesReadingsOutsideRequestedPeriodAndReportsMissingMetrics()
    {
        var from = DateTimeOffset.Parse("2026-06-18T00:00:00Z");
        var result = DiagnosisSummary.Create(from, from.AddDays(1), [
            new("temperature", 27, "C", from.AddHours(1)),
            new("temperature", 1, "C", DateTimeOffset.Parse("1970-01-01T00:06:17Z"))
        ]);
        Assert.Equal(1, result.ExcludedCount);
        Assert.Equal(27, Assert.Single(result.Metrics).Average);
        Assert.Contains("soilMoisture", result.MissingVariables);
    }

    [Fact]
    public void RejectsInvalidUnitsAndOutOfRangeHumidity()
    {
        var from = DateTimeOffset.Parse("2026-06-18T00:00:00Z");
        Assert.Throws<ArgumentException>(() => DiagnosisSummary.Create(from, from.AddDays(1), [new("humidity", 101, "%", from)]));
        Assert.Throws<ArgumentException>(() => DiagnosisSummary.Create(from, from.AddDays(1), [new("temperature", 27, "lux", from)]));
    }
}
