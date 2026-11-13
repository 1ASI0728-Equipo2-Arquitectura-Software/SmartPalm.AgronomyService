using SmartPalm.AgronomyService.Application.Reconciliation;

namespace SmartPalm.AgronomyService.API.Tests.Domain;

public class ProjectionReconciliationResultTests
{
    [Fact]
    public void TotalChanges_IncludesCreatedUpdatedAndRemovedProjections()
    {
        var result = new ProjectionReconciliationResult(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9,
            10, 11, 12);

        Assert.Equal(78, result.TotalChanges);
    }
}
