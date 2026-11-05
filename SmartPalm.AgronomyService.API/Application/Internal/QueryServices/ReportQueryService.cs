using SmartPalm.AgronomyService.Domain.Model.Aggregates;
using SmartPalm.AgronomyService.Domain.Model.Queries;
using SmartPalm.AgronomyService.Domain.Repositories;
using SmartPalm.AgronomyService.Domain.Services;

namespace SmartPalm.AgronomyService.Application.QueryServices;

public class ReportQueryService(ITechnicalReportRepository repository) : IReportQueryService
{
    public Task<IReadOnlyList<TechnicalReport>> HandleAsync(GetSectorReportsQuery q, CancellationToken t) => repository.ListBySectorAsync(q.SectorId, t);
    public Task<TechnicalReport?> HandleAsync(GetReportByIdQuery q, CancellationToken t) => repository.FindAsync(q.ReportId, t);
}
