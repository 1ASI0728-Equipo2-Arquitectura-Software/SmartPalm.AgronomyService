namespace SmartPalm.AgronomyService.Domain.Model.Events;

public record TechnicalReportPublishedIntegrationEvent(long ReportId, long AgronomistId, long SectorId, string Title, int Version, DateTime OccurredAtUtc);
