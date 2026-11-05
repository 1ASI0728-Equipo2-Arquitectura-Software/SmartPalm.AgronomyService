using SmartPalm.AgronomyService.Domain.Model.ValueObjects;

namespace SmartPalm.AgronomyService.Domain.Model.Commands;

public record CreateRecommendationCommand(long? SectorId, long AgronomistId, string Content, RecommendationType Type, long? ReportId);
