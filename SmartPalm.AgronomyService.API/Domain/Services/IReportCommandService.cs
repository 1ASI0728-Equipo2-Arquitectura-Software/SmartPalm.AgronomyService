using SmartPalm.AgronomyService.Domain.Model.Aggregates;
using SmartPalm.AgronomyService.Domain.Model.Commands;

namespace SmartPalm.AgronomyService.Domain.Services;

public interface IReportCommandService
{
    Task<TechnicalReport> HandleAsync(CreateReportCommand command, CancellationToken token);
    Task<TechnicalReport> HandleAsync(UpdateReportContentCommand command, CancellationToken token);
    Task<TechnicalReport> HandleAsync(PublishReportCommand command, CancellationToken token);
}
