using SmartPalm.AgronomyService.Domain.Model.Aggregates;
using SmartPalm.AgronomyService.Domain.Model.ValueObjects;

namespace SmartPalm.AgronomyService.Domain.Repositories;

public interface IRecommendationRepository
{
    Task<Recommendation?> FindAsync(long id, CancellationToken token);
    Task<IReadOnlyList<Recommendation>> ListBySectorAsync(long sectorId, RecommendationStatus? status, long? agronomistId, CancellationToken token);
    Task<IReadOnlyList<Recommendation>> ListGeneralAsync(RecommendationStatus? status, long? agronomistId, CancellationToken token);
    Task<IReadOnlyList<Recommendation>> ListByReportAsync(long reportId, CancellationToken token);
    void Add(Recommendation recommendation);
}
