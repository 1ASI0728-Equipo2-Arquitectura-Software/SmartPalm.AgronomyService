namespace SmartPalm.AgronomyService.Domain.Model.Events;

public record RecommendationPublishedIntegrationEvent(long RecommendationId, long AgronomistId, long? SectorId, string Content, string Type, int Version, DateTime OccurredAtUtc);
