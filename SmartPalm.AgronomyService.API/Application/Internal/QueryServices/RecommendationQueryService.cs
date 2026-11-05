using SmartPalm.AgronomyService.Domain.Model.Aggregates;
using SmartPalm.AgronomyService.Domain.Model.Queries;
using SmartPalm.AgronomyService.Domain.Repositories;
using SmartPalm.AgronomyService.Domain.Services;

namespace SmartPalm.AgronomyService.Application.QueryServices;

public class RecommendationQueryService(IRecommendationRepository repository) : IRecommendationQueryService
{
    public Task<IReadOnlyList<Recommendation>> HandleAsync(GetSectorRecommendationsQuery q, CancellationToken t) => repository.ListBySectorAsync(q.SectorId, q.Status, q.AgronomistId, t);
    public Task<IReadOnlyList<Recommendation>> HandleAsync(GetGeneralRecommendationsQuery q, CancellationToken t) => repository.ListGeneralAsync(q.Status, q.AgronomistId, t);
    public Task<IReadOnlyList<Recommendation>> HandleAsync(GetRecommendationsByReportIdQuery q, CancellationToken t) => repository.ListByReportAsync(q.ReportId, t);
    public Task<Recommendation?> HandleAsync(GetRecommendationByIdQuery q, CancellationToken t) => repository.FindAsync(q.RecommendationId, t);
}
