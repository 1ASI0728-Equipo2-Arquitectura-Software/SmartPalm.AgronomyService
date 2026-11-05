namespace SmartPalm.AgronomyService.Domain.Model.Events;

public record RecommendationCreatedIntegrationEvent(long RecommendationId, long AgronomistId, long? SectorId, string Type, int Version, DateTime OccurredAtUtc);
