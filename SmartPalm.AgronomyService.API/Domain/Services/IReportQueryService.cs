using SmartPalm.AgronomyService.Domain.Model.Aggregates;
using SmartPalm.AgronomyService.Domain.Model.Queries;

namespace SmartPalm.AgronomyService.Domain.Services;

public interface IReportQueryService
{
    Task<IReadOnlyList<TechnicalReport>> HandleAsync(GetSectorReportsQuery query, CancellationToken token);
    Task<TechnicalReport?> HandleAsync(GetReportByIdQuery query, CancellationToken token);
}
