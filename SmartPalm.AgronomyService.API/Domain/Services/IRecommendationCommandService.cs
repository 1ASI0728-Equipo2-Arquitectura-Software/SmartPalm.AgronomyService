using SmartPalm.AgronomyService.Domain.Model.Aggregates;
using SmartPalm.AgronomyService.Domain.Model.Commands;

namespace SmartPalm.AgronomyService.Domain.Services;

public interface IRecommendationCommandService
{
    Task<Recommendation> HandleAsync(CreateRecommendationCommand command, CancellationToken token);
    Task<Recommendation> HandleAsync(UpdateRecommendationContentCommand command, CancellationToken token);
    Task<Recommendation> HandleAsync(ApproveRecommendationCommand command, CancellationToken token);
    Task<Recommendation> HandleAsync(PublishRecommendationCommand command, CancellationToken token);
}
