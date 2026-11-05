using SmartPalm.AgronomyService.Domain.Model.Aggregates;
using SmartPalm.AgronomyService.Domain.Model.Commands;
using SmartPalm.AgronomyService.Domain.Model.ValueObjects;
using SmartPalm.AgronomyService.Interfaces.REST.Resources;

namespace SmartPalm.AgronomyService.Interfaces.REST.Transform;

public static class AgronomyAssemblers
{
    public static CreateRecommendationCommand ToCommand(long? sectorId, CreateRecommendationResource resource) =>
        new(sectorId, resource.agronomistId, resource.content, sectorId.HasValue ? RecommendationType.SectorSpecific : RecommendationType.General, resource.reportId);
    public static RecommendationResource ToResource(Recommendation value) => new(value.Id, value.Content, value.Type.ToString(), value.Status.ToString(), value.CreatedAtUtc, value.ApprovedAtUtc, value.PublishedAtUtc);
    public static CreateReportCommand ToCommand(long sectorId, CreateReportResource resource) => new(resource.agronomistId, sectorId, resource.title, resource.content, resource.interventionId, resource.findings);
    public static ReportResource ToResource(TechnicalReport value) => new(value.Id, value.AgronomistId, value.SectorId, value.InterventionId,
        value.Title, value.Content, value.Findings, value.Status.ToString(), value.CreatedAtUtc.ToString("o"), value.PublishedAtUtc?.ToString("o"));
    public static RecommendationStatus? ParseRecommendationStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return null;
        return Enum.TryParse<RecommendationStatus>(status, true, out var value) ? value : throw new ArgumentException("Invalid recommendation status.");
    }
}

