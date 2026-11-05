using Microsoft.EntityFrameworkCore;
using SmartPalm.AgronomyService.Domain.Model.Aggregates;
using SmartPalm.AgronomyService.Domain.Model.ValueObjects;
using SmartPalm.AgronomyService.Domain.Repositories;

namespace SmartPalm.AgronomyService.Infrastructure.Persistence;

internal class RecommendationRepository(AgronomyDbContext db) : IRecommendationRepository
{
    public Task<Recommendation?> FindAsync(long id, CancellationToken t) => db.Recommendations.SingleOrDefaultAsync(x => x.Id == id, t);
    public async Task<IReadOnlyList<Recommendation>> ListBySectorAsync(long sectorId, RecommendationStatus? status, long? agronomistId, CancellationToken t)
    {
        var q = db.Recommendations.AsNoTracking().Where(x => x.SectorId == sectorId);
        if (status is not null) q = q.Where(x => x.Status == status); if (agronomistId is not null) q = q.Where(x => x.AgronomistId == agronomistId);
        return await q.OrderByDescending(x => x.CreatedAtUtc).ToArrayAsync(t);
    }
    public async Task<IReadOnlyList<Recommendation>> ListGeneralAsync(RecommendationStatus? status, long? agronomistId, CancellationToken t)
    {
        var q = db.Recommendations.AsNoTracking().Where(x => x.Type == RecommendationType.General);
        if (status is not null) q = q.Where(x => x.Status == status); if (agronomistId is not null) q = q.Where(x => x.AgronomistId == agronomistId);
        return await q.OrderByDescending(x => x.CreatedAtUtc).ToArrayAsync(t);
    }
    public async Task<IReadOnlyList<Recommendation>> ListByReportAsync(long reportId, CancellationToken t) => await db.Recommendations.AsNoTracking().Where(x => x.ReportId == reportId).OrderByDescending(x => x.CreatedAtUtc).ToArrayAsync(t);
    public void Add(Recommendation value) => db.Recommendations.Add(value);
}
