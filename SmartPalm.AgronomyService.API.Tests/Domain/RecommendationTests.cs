using SmartPalm.AgronomyService.Domain.Model.Aggregates;
using SmartPalm.AgronomyService.Domain.Model.ValueObjects;

namespace SmartPalm.AgronomyService.API.Tests.Domain;

public class RecommendationTests
{
    [Fact]
    public void Recommendation_FollowsPendingApprovedPublishedWorkflow()
    {
        var value = new Recommendation(12, 5, "Apply irrigation", RecommendationType.SectorSpecific);
        Assert.Equal(RecommendationStatus.Pending, value.Status); value.Approve(); Assert.Equal(RecommendationStatus.Approved, value.Status);
        value.Publish(); Assert.Equal(RecommendationStatus.Published, value.Status); Assert.NotNull(value.PublishedAtUtc);
        Assert.Throws<InvalidOperationException>(() => value.UpdateContent("Change"));
    }
    [Fact]
    public void UpdatingApprovedRecommendation_RequiresNewApprovalAndIncrementsVersion()
    {
        var value = new Recommendation(null, 5, "Initial", RecommendationType.General); value.Approve(); value.UpdateContent("Revised");
        Assert.Equal(RecommendationStatus.Pending, value.Status); Assert.Equal(2, value.Version); Assert.Null(value.ApprovedAtUtc);
    }
    [Fact]
    public void SectorSpecificRecommendation_RequiresSector() => Assert.Throws<ArgumentException>(() =>
        new Recommendation(null, 5, "Invalid", RecommendationType.SectorSpecific));
}
