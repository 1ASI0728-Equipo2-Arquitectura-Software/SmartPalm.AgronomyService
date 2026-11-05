namespace SmartPalm.AgronomyService.Interfaces.REST.Resources;

public record CreateRecommendationResource(long agronomistId, string content, long? reportId = null);
