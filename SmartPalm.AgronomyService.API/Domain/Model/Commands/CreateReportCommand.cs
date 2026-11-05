namespace SmartPalm.AgronomyService.Domain.Model.Commands;

public record CreateReportCommand(long AgronomistId, long SectorId, string Title, string Content, long? InterventionId, string? Findings);
