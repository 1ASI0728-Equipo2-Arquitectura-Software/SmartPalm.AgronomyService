using Microsoft.EntityFrameworkCore;
using SmartPalm.AgronomyService.Domain.Model.Aggregates;
using SmartPalm.AgronomyService.Domain.Repositories;

namespace SmartPalm.AgronomyService.Infrastructure.Persistence;

internal class TechnicalReportRepository(AgronomyDbContext db) : ITechnicalReportRepository
{
    public Task<TechnicalReport?> FindAsync(long id, CancellationToken t) => db.TechnicalReports.SingleOrDefaultAsync(x => x.Id == id, t);
    public async Task<IReadOnlyList<TechnicalReport>> ListBySectorAsync(long sectorId, CancellationToken t) => await db.TechnicalReports.AsNoTracking().Where(x => x.SectorId == sectorId).OrderByDescending(x => x.CreatedAtUtc).ToArrayAsync(t);
    public void Add(TechnicalReport value) => db.TechnicalReports.Add(value);
}
