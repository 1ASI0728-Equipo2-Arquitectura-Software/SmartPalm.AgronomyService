namespace SmartPalm.AgronomyService.Domain.Model.Commands;

public record UpdateReportContentCommand(long ReportId, string Title, string Content, string? Findings);
