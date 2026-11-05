using SmartPalm.AgronomyService.Domain.Model.ValueObjects;

namespace SmartPalm.AgronomyService.Domain.Model.Queries;

public record GetGeneralRecommendationsQuery(RecommendationStatus? Status, long? AgronomistId);
