namespace SmartPalm.AgronomyService.Interfaces.REST.Resources;

public record CreateReportResource(long agronomistId, string title, string content, long? interventionId, string? findings);
