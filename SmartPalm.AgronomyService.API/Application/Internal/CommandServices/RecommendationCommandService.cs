using SmartPalm.AgronomyService.Application.Ports;
using SmartPalm.AgronomyService.Application.DomainServices;
using SmartPalm.AgronomyService.Domain.Model.Aggregates;
using SmartPalm.AgronomyService.Domain.Model.Commands;
using SmartPalm.AgronomyService.Domain.Model.Events;
using SmartPalm.AgronomyService.Domain.Model.ValueObjects;
using SmartPalm.AgronomyService.Domain.Repositories;
using SmartPalm.AgronomyService.Domain.Services;

namespace SmartPalm.AgronomyService.Application.CommandServices;

public class RecommendationCommandService(IRecommendationRepository recommendations, ITechnicalReportRepository reports, AgronomyAuthorizationService authorization,
    IUnitOfWork unitOfWork, IIntegrationEventWriter events) : IRecommendationCommandService
{
    public Task<Recommendation> HandleAsync(CreateRecommendationCommand command, CancellationToken token) => unitOfWork.ExecuteAsync(async ct =>
    {
        await authorization.EnsureAuthorAsync(command.AgronomistId, command.SectorId, ct);
        if (command.ReportId is not null && await reports.FindAsync(command.ReportId.Value, ct) is null)
            throw new ArgumentException("Report does not exist.");
        var recommendation = new Recommendation(command.SectorId, command.AgronomistId, command.Content, command.Type, command.ReportId);
        recommendations.Add(recommendation); await unitOfWork.SaveChangesAsync(ct);
        events.Enqueue("agronomy.recommendation-created.v1", new RecommendationCreatedIntegrationEvent(
            recommendation.Id, recommendation.AgronomistId, recommendation.SectorId, recommendation.Type.ToString(), recommendation.Version, DateTime.UtcNow));
        await unitOfWork.SaveChangesAsync(ct); return recommendation;
    }, token);

    public Task<Recommendation> HandleAsync(UpdateRecommendationContentCommand command, CancellationToken token) => unitOfWork.ExecuteAsync(async ct =>
    {
        var item = await GetAsync(command.RecommendationId, ct); await authorization.EnsureAuthorAsync(item.AgronomistId, item.SectorId, ct);
        item.UpdateContent(command.Content); await unitOfWork.SaveChangesAsync(ct); return item;
    }, token);

    public Task<Recommendation> HandleAsync(ApproveRecommendationCommand command, CancellationToken token) => unitOfWork.ExecuteAsync(async ct =>
    {
        var item = await GetAsync(command.RecommendationId, ct); await authorization.EnsureAuthorAsync(item.AgronomistId, item.SectorId, ct); item.Approve();
        events.Enqueue("agronomy.recommendation-approved.v1", new RecommendationApprovedIntegrationEvent(item.Id, item.AgronomistId, item.Version, DateTime.UtcNow));
        await unitOfWork.SaveChangesAsync(ct); return item;
    }, token);

    public Task<Recommendation> HandleAsync(PublishRecommendationCommand command, CancellationToken token) => unitOfWork.ExecuteAsync(async ct =>
    {
        var item = await GetAsync(command.RecommendationId, ct); await authorization.EnsureAuthorAsync(item.AgronomistId, item.SectorId, ct); item.Publish();
        events.Enqueue("agronomy.recommendation-published.v1", new RecommendationPublishedIntegrationEvent(
            item.Id, item.AgronomistId, item.SectorId, item.Content, item.Type.ToString(), item.Version, DateTime.UtcNow));
        await unitOfWork.SaveChangesAsync(ct); return item;
    }, token);

    private async Task<Recommendation> GetAsync(long id, CancellationToken token) =>
        await recommendations.FindAsync(id, token) ?? throw new KeyNotFoundException("Recommendation not found.");
}
