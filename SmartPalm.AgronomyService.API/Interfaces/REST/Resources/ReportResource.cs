namespace SmartPalm.AgronomyService.Interfaces.REST.Resources;

public record ReportResource(long id, long agronomistId, long sectorId, long? interventionId, string title, string content,
    string? findings, string status, string createdAt, string? publishedAt);
