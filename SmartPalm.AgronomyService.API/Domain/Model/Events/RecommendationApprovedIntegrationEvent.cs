namespace SmartPalm.AgronomyService.Domain.Model.Events;

public record RecommendationApprovedIntegrationEvent(long RecommendationId, long AgronomistId, int Version, DateTime OccurredAtUtc);
