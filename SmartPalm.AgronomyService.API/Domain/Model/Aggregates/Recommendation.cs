using SmartPalm.AgronomyService.Domain.Model.ValueObjects;

namespace SmartPalm.AgronomyService.Domain.Model.Aggregates;

public class Recommendation
{
    private Recommendation() { Content = string.Empty; }
    public Recommendation(long? sectorId, long agronomistId, string content, RecommendationType type, long? reportId = null)
    {
        if (agronomistId <= 0) throw new ArgumentException("AgronomistId must be greater than zero.");
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Recommendation content cannot be empty.");
        if (content.Trim().Length > 500) throw new ArgumentException("Recommendation content cannot exceed 500 characters.");
        if (type == RecommendationType.SectorSpecific && sectorId is null) throw new ArgumentException("SectorId is required for sector-specific recommendations.");
        if (type == RecommendationType.General && sectorId is not null) throw new ArgumentException("General recommendations cannot reference a sector.");
        SectorId = sectorId; AgronomistId = agronomistId; Content = content.Trim(); Type = type; ReportId = reportId;
        Status = RecommendationStatus.Pending; CreatedAtUtc = DateTime.UtcNow; Version = 1;
    }
    public long Id { get; private set; }
    public long? SectorId { get; private set; }
    public long? ReportId { get; private set; }
    public long AgronomistId { get; private set; }
    public string Content { get; private set; }
    public RecommendationType Type { get; private set; }
    public RecommendationStatus Status { get; private set; }
    public int Version { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ApprovedAtUtc { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }

    public void UpdateContent(string content)
    {
        if (Status == RecommendationStatus.Published) throw new InvalidOperationException("A published recommendation cannot be updated.");
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Recommendation content cannot be empty.");
        if (content.Trim().Length > 500) throw new ArgumentException("Recommendation content cannot exceed 500 characters.");
        Content = content.Trim(); Status = RecommendationStatus.Pending; ApprovedAtUtc = null; Version++;
    }
    public void Approve()
    {
        if (Status != RecommendationStatus.Pending) throw new InvalidOperationException("Only pending recommendations can be approved.");
        Status = RecommendationStatus.Approved; ApprovedAtUtc = DateTime.UtcNow;
    }
    public void Publish()
    {
        if (Status != RecommendationStatus.Approved) throw new InvalidOperationException("Only approved recommendations can be published.");
        Status = RecommendationStatus.Published; PublishedAtUtc = DateTime.UtcNow;
    }
}
