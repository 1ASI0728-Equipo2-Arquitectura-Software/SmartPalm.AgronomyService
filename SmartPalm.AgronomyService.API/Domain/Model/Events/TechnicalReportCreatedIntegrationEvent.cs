namespace SmartPalm.AgronomyService.Domain.Model.Events;

public record TechnicalReportCreatedIntegrationEvent(long ReportId, long AgronomistId, long SectorId, int Version, DateTime OccurredAtUtc);
