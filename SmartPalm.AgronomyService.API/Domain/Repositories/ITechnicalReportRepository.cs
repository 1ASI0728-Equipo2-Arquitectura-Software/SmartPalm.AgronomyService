using SmartPalm.AgronomyService.Domain.Model.Aggregates;

namespace SmartPalm.AgronomyService.Domain.Repositories;

public interface ITechnicalReportRepository
{
    Task<TechnicalReport?> FindAsync(long id, CancellationToken token);
    Task<IReadOnlyList<TechnicalReport>> ListBySectorAsync(long sectorId, CancellationToken token);
    void Add(TechnicalReport report);
}
