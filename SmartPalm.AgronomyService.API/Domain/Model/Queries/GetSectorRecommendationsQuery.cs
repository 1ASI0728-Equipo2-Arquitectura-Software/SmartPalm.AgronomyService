using SmartPalm.AgronomyService.Domain.Model.ValueObjects;

namespace SmartPalm.AgronomyService.Domain.Model.Queries;

public record GetSectorRecommendationsQuery(long SectorId, RecommendationStatus? Status, long? AgronomistId);
