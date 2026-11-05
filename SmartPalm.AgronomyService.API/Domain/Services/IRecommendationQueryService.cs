using SmartPalm.AgronomyService.Domain.Model.Aggregates;
using SmartPalm.AgronomyService.Domain.Model.Queries;

namespace SmartPalm.AgronomyService.Domain.Services;

public interface IRecommendationQueryService
{
    Task<IReadOnlyList<Recommendation>> HandleAsync(GetSectorRecommendationsQuery query, CancellationToken token);
    Task<IReadOnlyList<Recommendation>> HandleAsync(GetGeneralRecommendationsQuery query, CancellationToken token);
    Task<IReadOnlyList<Recommendation>> HandleAsync(GetRecommendationsByReportIdQuery query, CancellationToken token);
    Task<Recommendation?> HandleAsync(GetRecommendationByIdQuery query, CancellationToken token);
}
