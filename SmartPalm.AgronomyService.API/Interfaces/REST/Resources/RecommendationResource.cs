namespace SmartPalm.AgronomyService.Interfaces.REST.Resources;

public record RecommendationResource(long id, string content, string type, string status, DateTime createdAt, DateTime? approvedAt, DateTime? publishedAt);
