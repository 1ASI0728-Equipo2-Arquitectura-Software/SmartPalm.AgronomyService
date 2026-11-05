namespace SmartPalm.AgronomyService.Domain.Model.Commands;

public record UpdateRecommendationContentCommand(long RecommendationId, string Content);
