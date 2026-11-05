using SmartPalm.AgronomyService.Application.Ports;
using SmartPalm.AgronomyService.Application.DomainServices;
using SmartPalm.AgronomyService.Domain.Model.Aggregates;
using SmartPalm.AgronomyService.Domain.Model.Commands;
using SmartPalm.AgronomyService.Domain.Model.Events;
using SmartPalm.AgronomyService.Domain.Repositories;
using SmartPalm.AgronomyService.Domain.Services;

namespace SmartPalm.AgronomyService.Application.CommandServices;

public class ReportCommandService(ITechnicalReportRepository reports, AgronomyAuthorizationService authorization,
    IUnitOfWork unitOfWork, IIntegrationEventWriter events) : IReportCommandService
{
    public Task<TechnicalReport> HandleAsync(CreateReportCommand command, CancellationToken token) => unitOfWork.ExecuteAsync(async ct =>
    {
        await authorization.EnsureAuthorAsync(command.AgronomistId, command.SectorId, ct);
        var report = new TechnicalReport(command.AgronomistId, command.SectorId, command.Title, command.Content, command.InterventionId, command.Findings);
        reports.Add(report); await unitOfWork.SaveChangesAsync(ct);
        events.Enqueue("agronomy.report-created.v1", new TechnicalReportCreatedIntegrationEvent(report.Id, report.AgronomistId, report.SectorId, report.Version, DateTime.UtcNow));
        await unitOfWork.SaveChangesAsync(ct); return report;
    }, token);
    public Task<TechnicalReport> HandleAsync(UpdateReportContentCommand command, CancellationToken token) => unitOfWork.ExecuteAsync(async ct =>
    {
        var report = await GetAsync(command.ReportId, ct); await authorization.EnsureAuthorAsync(report.AgronomistId, report.SectorId, ct);
        report.UpdateContent(command.Title, command.Content, command.Findings); await unitOfWork.SaveChangesAsync(ct); return report;
    }, token);
    public Task<TechnicalReport> HandleAsync(PublishReportCommand command, CancellationToken token) => unitOfWork.ExecuteAsync(async ct =>
    {
        var report = await GetAsync(command.ReportId, ct); await authorization.EnsureAuthorAsync(report.AgronomistId, report.SectorId, ct); report.Publish();
        events.Enqueue("agronomy.report-published.v1", new TechnicalReportPublishedIntegrationEvent(
            report.Id, report.AgronomistId, report.SectorId, report.Title, report.Version, DateTime.UtcNow));
        await unitOfWork.SaveChangesAsync(ct); return report;
    }, token);
    private async Task<TechnicalReport> GetAsync(long id, CancellationToken token) =>
        await reports.FindAsync(id, token) ?? throw new KeyNotFoundException("Report not found.");
}
