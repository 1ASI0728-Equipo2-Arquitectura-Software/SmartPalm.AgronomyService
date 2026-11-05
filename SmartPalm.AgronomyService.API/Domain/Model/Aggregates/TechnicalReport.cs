using SmartPalm.AgronomyService.Domain.Model.ValueObjects;

namespace SmartPalm.AgronomyService.Domain.Model.Aggregates;

public class TechnicalReport
{
    private TechnicalReport() { Title = Content = string.Empty; }
    public TechnicalReport(long agronomistId, long sectorId, string title, string content, long? interventionId, string? findings)
    {
        if (agronomistId <= 0) throw new ArgumentException("AgronomistId must be greater than zero.");
        if (sectorId <= 0) throw new ArgumentException("SectorId must be greater than zero.");
        Validate(title, content);
        AgronomistId = agronomistId; SectorId = sectorId; Title = title.Trim(); Content = content.Trim();
        InterventionId = interventionId; Findings = findings?.Trim(); Status = ReportStatus.Draft; CreatedAtUtc = DateTime.UtcNow; Version = 1;
    }
    public long Id { get; private set; }
    public long AgronomistId { get; private set; }
    public long SectorId { get; private set; }
    public long? InterventionId { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public string? Findings { get; private set; }
    public ReportStatus Status { get; private set; }
    public int Version { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public void UpdateContent(string title, string content, string? findings)
    {
        if (Status == ReportStatus.Published) throw new InvalidOperationException("A published report cannot be updated.");
        Validate(title, content); Title = title.Trim(); Content = content.Trim(); Findings = findings?.Trim(); Version++;
    }
    public void Publish()
    {
        if (Status != ReportStatus.Draft) throw new InvalidOperationException("Only draft reports can be published.");
        Status = ReportStatus.Published; PublishedAtUtc = DateTime.UtcNow;
    }
    private static void Validate(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty.");
        if (title.Trim().Length > 200) throw new ArgumentException("Title cannot exceed 200 characters.");
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content cannot be empty.");
    }
}
