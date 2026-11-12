namespace SmartPalm.AgronomyService.Domain.Services;

public sealed record DiagnosisReading(string Variable, double Value, string Unit, DateTimeOffset MeasuredAt);
public sealed record DiagnosisMetric(string Variable, string Unit, int Count, double Minimum, double Maximum, double Average, double Latest, DateTimeOffset LatestAt);

public sealed record DiagnosisSummary(DateTimeOffset From, DateTimeOffset To, IReadOnlyList<DiagnosisMetric> Metrics, int ExcludedCount, IReadOnlyList<string> MissingVariables)
{
    private static readonly string[] Variables = ["temperature", "humidity", "soilMoisture", "light"];

    public static DiagnosisSummary Create(DateTimeOffset from, DateTimeOffset to, IReadOnlyCollection<DiagnosisReading> readings)
    {
        if (from >= to) throw new ArgumentException("The start must be before the end of the analysis period.");
        if (readings is null || readings.Count is 0 or > 10000) throw new ArgumentException("Provide between 1 and 10000 readings.");
        foreach (var reading in readings)
        {
            if (reading is null || !double.IsFinite(reading.Value) || reading.MeasuredAt == default)
                throw new ArgumentException("Each reading must have a finite value and a timestamp.");
            var valid = reading.Variable switch
            {
                "temperature" => reading.Unit == "C" && reading.Value is >= -50 and <= 80,
                "humidity" or "soilMoisture" => reading.Unit == "%" && reading.Value is >= 0 and <= 100,
                "light" => reading.Unit == "lux" && reading.Value >= 0 || reading.Unit == "%" && reading.Value is >= 0 and <= 100,
                _ => false
            };
            if (!valid) throw new ArgumentException("A reading has an unsupported variable, unit, or value.");
        }
        var included = readings.Where(x => x.MeasuredAt >= from && x.MeasuredAt <= to).ToArray();
        if (included.Length == 0) throw new ArgumentException("No readings fall within the requested period.");
        var metrics = included.GroupBy(x => (x.Variable, x.Unit)).Select(group =>
        {
            var latest = group.OrderByDescending(x => x.MeasuredAt).First();
            return new DiagnosisMetric(group.Key.Variable, group.Key.Unit, group.Count(), group.Min(x => x.Value), group.Max(x => x.Value), group.Average(x => x.Value), latest.Value, latest.MeasuredAt);
        }).ToArray();
        return new DiagnosisSummary(from, to, metrics, readings.Count - included.Length, Variables.Except(included.Select(x => x.Variable)).ToArray());
    }
}
